﻿using CosmosResourceToken.Core.Broker;
using CosmosResourceToken.Core.Model;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static CosmosResourceToken.Core.Model.Constants;

namespace CosmosResourceTokenBroker
{
    public class ResourceTokenBrokerService : IResourceTokenBrokerService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly string _databaseId;

        private readonly Container _container;
        private readonly string _collectionId;
        private readonly string _endpointUrl;
        private string _partitionKeyHeader;

        private readonly TimeSpan _resourceTokenTtl;

        private static string GetPermissionUserId(string userId, IPermissionScope permissionScope) => $"{userId}-{permissionScope.Scope}";

        public ResourceTokenBrokerService(
            string endpointUrl, 
            string key, 
            string databaseId, 
            string collectionId,
            TimeSpan? resourceTokenTtl = default)
        {
            _endpointUrl = endpointUrl;
            _databaseId = databaseId;
            _collectionId = collectionId;

            _cosmosClient = new CosmosClient(endpointUrl, key);

            _database = _cosmosClient.GetDatabase(databaseId);

            _container = _database.GetContainer(collectionId);

            // Default is one hour - i.e. 3600 seconds
            if (resourceTokenTtl is null)
            {
                _resourceTokenTtl = TimeSpan.FromHours(1);
            }
        }

        public async Task<IResourcePermissionResponse> Get(
            string userId,
            IEnumerable<IPermissionScope> permissionScopes, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var permissionUsers = await GetOrCreatePermissionUsers(userId, cancellationToken);

                return await GetOrCreatePermissions(
                    permissionUsers,
                    userId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to acquire Resource Permission from Azure Cosmos DB. Unhandled exception: {ex}");
            }
        }

        private async Task<IEnumerable<(User user, IPermissionScope permissionScope)>> GetOrCreatePermissionUsers(string userId, CancellationToken ct)
        {
            // Create a user creation task for each of the permission type - i.e. a user for read-only, a user for read-write etc.
            var permissionUserTasks = KnownPermissionScopes
                .Select(permissionScope => GetOrCreateUser(userId, permissionScope, ct));

            // Run user creations in parallel. 
            return await Task.WhenAll(permissionUserTasks);
        }

        private async Task<(User user, IPermissionScope permissionScope)> GetOrCreateUser(string userId, IPermissionScope permissionScope, CancellationToken ct)
        {
            var permissionUserId = GetPermissionUserId(userId, permissionScope);

            try
            {
                var user = _database.GetUser(permissionUserId);

                var userResponse = await user.ReadAsync(cancellationToken: ct);

                // If the user does not exist, then create it.
                // This if statement is probably not necessary, as an CosmosException is throw if the user.ReadAsync fails:
                // however the documentation is not 100 % clear on this: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.database.readasync?view=azure-dotnet
                if (userResponse?.StatusCode != HttpStatusCode.OK || userResponse?.User is null)
                {
                    userResponse = await _database.CreateUserAsync(permissionUserId, cancellationToken: ct);
                }

                return (userResponse.User, permissionScope);
            }
            catch (CosmosException ex)
            {
                // If the user does not exist, then create it.
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var user = await _database.CreateUserAsync(permissionUserId, cancellationToken: ct);

                    return (user, permissionScope);
                }

                throw new ResourceTokenBrokerServiceException($"Unable to get or create user with user id: {permissionUserId}. Unhandled exception: {ex}");
            }
        }

        private async Task<IResourcePermissionResponse> GetOrCreatePermissions(
            IEnumerable<(User user, IPermissionScope permissionScope)> usersWithPermisssionScope,
            string userId,
            CancellationToken ct)
        {
            _partitionKeyHeader = await GetPartionKeyHeader(ct);

            var getOrCreateUserPermissionsTask = usersWithPermisssionScope
                .Select(tuple => GetOrCreateUserPermission(tuple.user, userId, tuple.permissionScope, ct));

            var permissions = await Task.WhenAll(getOrCreateUserPermissionsTask);
            
            return new ResourcePermissionResponse(permissions, userId, _endpointUrl, _databaseId, _collectionId, _partitionKeyHeader);
        }


        private async Task<string> GetPartionKeyHeader(CancellationToken ct)
        {
            try
            {
                var containerResponse = await _container.ReadContainerAsync(cancellationToken: ct);

                if (containerResponse?.StatusCode == HttpStatusCode.OK)
                {
                    return containerResponse.Resource.PartitionKeyPath.Replace("/", string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable acquire Partition Key Header from Cosmos container. Unhandled exception: {ex}");
            }
            
            throw new ResourceTokenBrokerServiceException($"Unable acquire Partition Key Header from Cosmos container");
        }

        private async Task<IResourcePermission> GetOrCreateUserPermission(
            User user, 
            string userId,
            IPermissionScope permissionScope,
            CancellationToken ct)
        {
            var permissionId = userId.ToPermissionIdBy(permissionScope.Scope);

            try
            {
                var permission = user.GetPermission(permissionId);

                var permissionResponse = await permission.ReadAsync(
                    Convert.ToInt32(_resourceTokenTtl.TotalSeconds),
                    cancellationToken: ct);

                // If the permission does not exist, then create it.
                // Now sure if this statement is necessary, as an CosmosException is likely thrown if the Permission.ReadAsync fails:
                // however the documentation is not 100 % clear on this: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.permission.readasync?view=azure-dotnet
                if (permissionResponse?.StatusCode != HttpStatusCode.OK 
                    || permissionResponse?.Resource?.ResourcePartitionKey.GetValueOrDefault() is null)
                {
                    return await UpsertPermission(user, userId, permissionId, permissionScope, ct);
                }

                // If the permission is wrongly configured, update it.
                if (permissionResponse?.Resource?.ResourcePartitionKey.ToPartitionKeyString() !=
                    userId.ToPartitionKeyBy(permissionScope.PermissionMode))
                {
                    return await UpsertPermission(user, userId, permissionId, permissionScope, ct);
                }

                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                var partitionKeyValueJson = permissionResponse.Resource.ResourcePartitionKey.GetValueOrDefault().ToString();

                string partitionKeyValue;

                try
                {
                    partitionKeyValue = JArray.Parse(partitionKeyValueJson)?.FirstOrDefault()?.Value<string>();

                    if (string.IsNullOrEmpty(partitionKeyValue))
                    {
                        throw new ArgumentNullException(partitionKeyValue);
                    }
                }
                catch (Exception ex)
                {
                    throw new ResourceTokenBrokerServiceException($"Unable to parse partition key from existing permission: {permissionId}. Unhandled exception: {ex}");
                }

                return new ResourcePermission(
                    permissionScope,
                    permissionResponse.Resource.Token,
                    permissionResponse.Resource.Id,
                    partitionKeyValue,
                    expiresUtc);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return await UpsertPermission(user, userId, permissionId, permissionScope, ct);
                }

                throw new ResourceTokenBrokerServiceException($"Unable to read or create user permissions. Unhandled exception: {ex}");
            }
        }

        private async Task<IResourcePermission> UpsertPermission(
            User user,
            string userId,
            string permissionId,
            IPermissionScope permissionScope,
            CancellationToken ct)
        {
            try
            {
                var partitionKeyValue = userId.ToPartitionKeyBy(permissionScope.PermissionMode);

                var partitionKey = new PartitionKey(partitionKeyValue);

                var permissionProperties = new PermissionProperties(
                    permissionId,
                    permissionScope.PermissionMode.ToCosmosPermissionMode(),
                    _container,
                    partitionKey);

                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                var permissionResponse = await user.UpsertPermissionAsync(
                    permissionProperties,
                    Convert.ToInt32(_resourceTokenTtl.TotalSeconds),
                    cancellationToken: ct);

                if (permissionResponse?.StatusCode == HttpStatusCode.OK
                    || permissionResponse?.StatusCode == HttpStatusCode.Created)
                {
                    if (!string.IsNullOrEmpty(permissionResponse?.Resource?.Token)
                        && !string.IsNullOrEmpty(permissionResponse?.Resource?.Id))
                    {
                        return new ResourcePermission(
                            permissionScope,
                            permissionResponse.Resource.Token,
                            permissionResponse.Resource.Id,
                            partitionKeyValue,
                            expiresUtc);
                    }
                }

                throw new ResourceTokenBrokerServiceException($"Unable to upsert permission for user. Token or Id is missing or invalid. Status code: {permissionResponse?.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to upsert new permission for user. Unhandled exception: {ex}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            
            await Task.CompletedTask;
        }
    }
}

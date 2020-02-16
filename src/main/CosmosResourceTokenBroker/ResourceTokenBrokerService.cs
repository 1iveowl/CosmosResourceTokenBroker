using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Broker;
using CosmosResourceToken.Core.Model;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using static CosmosResourceToken.Core.Model.Constants;

namespace CosmosResourceTokenBroker
{
    public class ResourceTokenBrokerService : IResourceTokenBrokerService
    {


        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly string _collectionId;
        private readonly string _endpointUrl;

        private readonly TimeSpan _resourceTokenTtl;

        public ResourceTokenBrokerService(
            string endpointUrl, 
            string key, 
            string databaseId, 
            string collectionId,
            TimeSpan? resourceTokenTtl = default)
        {
            _cosmosClient = new CosmosClient(endpointUrl, key);
            _database = _cosmosClient.GetDatabase(databaseId);
            _collectionId = collectionId;
            _endpointUrl = endpointUrl;

            if (resourceTokenTtl is null)
            {
                _resourceTokenTtl = TimeSpan.FromHours(1);
            }
        }

        public async Task<IResourcePermissionResponse> Get(
            string userId,
            IEnumerable<IPermissionScope> permissionscopes, 
            CancellationToken cancellationToken = default)
        {
            var user = await GetOrCreateCosmosUser(
                userId, 
                cancellationToken);

            return await GetOrCreateUserPermissions(
                user, 
                permissionscopes,
                cancellationToken);
        }

        private async Task<User> GetOrCreateCosmosUser(string userid, CancellationToken ct)
        {
            try
            {
                var user = _database.GetUser(userid);

                return user;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return await _database.CreateUserAsync(userid, cancellationToken: ct);
                }
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to get user. Unhandled exception: {ex}");
            }

            return null;
        }

        private async Task<IResourcePermissionResponse> GetOrCreateUserPermissions(
            User user, 
            IEnumerable<IPermissionScope> permissionscopes, 
            CancellationToken ct)
        {
            try
            {

                var readPermissionTasks = KnownPermissionScopes
                    .Select(kps => user.GetPermission(user.ToPermissionIdForScope(kps.Scope)))
                    .Select(permission => permission.ReadAsync(Convert.ToInt32(_resourceTokenTtl.TotalSeconds), cancellationToken:ct));

                var permissionResponses = await Task.WhenAll(readPermissionTasks);

                var resourcePermissions = permissionResponses
                    .Where(permissionResponse => !(permissionResponse?.Resource is null))
                    .Select(permissionResponse => permissionResponse.Resource)
                    .Select(permissionProperties =>
                    {
                        var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                        var permissionScope = permissionProperties.ToPermissionScope();

                        var permissionId = permissionProperties?.Id;
                        var resourceToken = permissionProperties?.Token;

                        return new ResourcePermission(permissionScope, resourceToken, permissionId, expiresUtc);
                    });

                return new ResourcePermissionResponse(resourcePermissions, user.Id, _endpointUrl);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new ResourceTokenBrokerServiceException($"Unable to read or create user permissions. Unhandled exception: {ex}");
                }
            }

            return await CreateNewPermissions(user, permissionscopes, ct);
        }


        private async Task<IResourcePermissionResponse> CreateNewPermissions(
            User user,
            IEnumerable<IPermissionScope> permissionScopes, 
            CancellationToken ct)
        {
            try
            {
                var container = _database.GetContainer(_collectionId);

                // Create a list of tasks CreatePermission
                var taskListPermissionCreate = permissionScopes
                    .Select(ps =>
                    {
                        var partitionKey = new PartitionKey(user.ToPartitionKeyFromPermissionMode(ps.PermissionMode));

                        var permissionProperties = new PermissionProperties(
                            user.ToPermissionIdForScope(ps.Scope),
                            ps.PermissionMode.ToCosmosPermissionMode(),
                            container,
                            partitionKey);
                        
                        return (partitionKey, permissionProperties, ps, ct, user, ps.Scope);
                    })
                    .Select(tuple => CreatePermissionResponse(tuple.user, container, tuple.ps, tuple.permissionProperties, tuple.partitionKey, tuple.ct));

                // Run list of task in parallel
                var resourcePermissions = await Task.WhenAll(taskListPermissionCreate);

                return new ResourcePermissionResponse
                {
                    UserId = user.Id,
                    EndpointUrl = _endpointUrl,
                    ResourcePermissions = resourcePermissions
                };
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Unhandled exception: {ex}");
            }
        }
        
        private async Task<IResourcePermission> CreatePermissionResponse(
            User user, 
            Container container,
            IPermissionScope permissionScope,
            PermissionProperties permissionProperties,
            PartitionKey partitionKey,
            CancellationToken ct)
        {
            try
            {
                var initializeObjectName = user.ToPartitionKeyFromPermissionMode(permissionScope.PermissionMode);

                await InitializeObjectForPartition(container, user, partitionKey, initializeObjectName, ct);
                
                var permissionResponse = await user.CreatePermissionAsync(
                    permissionProperties,
                    Convert.ToInt32(_resourceTokenTtl.TotalSeconds), 
                    cancellationToken: ct);

                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                if (!(permissionResponse?.Resource?.Token is null))
                {
                    return new ResourcePermission(permissionScope, permissionResponse.Resource.Token, permissionResponse.Resource.Token, expiresUtc);
                }

                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Status code: {permissionResponse?.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Unhandled exception: {ex}");
            }
        }

        private async Task InitializeObjectForPartition(Container container, User user, PartitionKey partitionKey, string initializeObjectName, CancellationToken ct)
        {
            try
            {
                var response = await container.ReadItemAsync<InitObject>(user.Id, partitionKey, cancellationToken:ct);

                if (!(response is null))
                {
                    return;
                }

            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new ResourceTokenBrokerServiceException($"Unhandled read item error, Status Code: {ex.StatusCode.ToString()}", ex);
                }
            }

            await CreateForPartition(container, user, partitionKey, initializeObjectName, ct);
        }

        private async Task CreateForPartition(Container container, User user, PartitionKey partitionKey, string initializeObjectName, CancellationToken ct)
        {
            try
            {
                var obj = new InitObject($"dummy-{initializeObjectName}", initializeObjectName);
                
                var item = await container.CreateItemAsync(obj, partitionKey, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException("Unable to initialize partition with object.", ex);
            }
        }
        
        public ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            
            return new ValueTask();
        }
    }

    internal static class Utility
    {
        internal static string ToPartitionKeyFromPermissionMode(this User user, PermissionModeKind permissionMode) => permissionMode == PermissionModeKind.SharedRead
            ? $"shared"
            : $"user-{user.Id}";

        internal static string ToPermissionIdForScope(this User user, string scope) => $"{user.Id}{PermissionScopePrefix}{scope}";

        internal static IPermissionScope ToPermissionScope(this PermissionProperties pp) =>
            KnownPermissionScopes?.FirstOrDefault(s => s?.Scope == pp.Id?.Split(PermissionScopePrefix)[1]);


        internal static PermissionMode ToCosmosPermissionMode(this PermissionModeKind permissionMode) => permissionMode switch
        {
            PermissionModeKind.UserReadWrite => PermissionMode.All,
            PermissionModeKind.UserRead => PermissionMode.Read,
            PermissionModeKind.SharedRead => PermissionMode.Read,
            _ => throw new ArgumentOutOfRangeException(nameof(permissionMode), permissionMode,
                "Unknown permission mode")
        };
    }
}

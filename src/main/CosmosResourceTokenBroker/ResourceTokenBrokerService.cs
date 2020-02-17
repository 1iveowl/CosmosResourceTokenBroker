using CosmosResourceToken.Core.Broker;
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
        private readonly string _collectionId;
        private readonly string _endpointUrl;

        private readonly TimeSpan _resourceTokenTtl;

        private static string GetPermissionUserId(string userId, IPermissionScope permissionScope) => $"{userId}-{permissionScope.Scope}";

        public ResourceTokenBrokerService(
            string endpointUrl, 
            string key, 
            string databaseId, 
            string collectionId,
            TimeSpan? resourceTokenTtl = default)
        {
            _collectionId = collectionId;
            _endpointUrl = endpointUrl;

            //var clientOptions = new CosmosClientOptions
            //{
            //    AllowBulkExecution = true
            //};

            _cosmosClient = new CosmosClient(endpointUrl, key);

            _database = _cosmosClient.GetDatabase(databaseId);

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
            var permissionUsers = await GetOrCreateUsers(
                userId, 
                cancellationToken);

            return await GetOrCreateUsersPermissions(
                permissionUsers,
                userId,
                cancellationToken);
        }

        private async Task<IEnumerable<(User user, IPermissionScope permissionScope)>> GetOrCreateUsers(string userId, CancellationToken ct)
        {
            var permissionUserTasks = KnownPermissionScopes
                .Select(permissionScope => GetOrCreateUser(userId, permissionScope, ct));

            return await Task.WhenAll(permissionUserTasks);
        }

        private async Task<(User user, IPermissionScope permissionScope)> GetOrCreateUser(string userId, IPermissionScope permissionScope, CancellationToken ct)
        {
            var permissionUserId = GetPermissionUserId(userId, permissionScope);

            try
            {
                var user = _database.GetUser(permissionUserId);

                var userProperties = await user.ReadAsync(cancellationToken: ct);

                return (userProperties.User, permissionScope);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var user = await _database.CreateUserAsync(permissionUserId, cancellationToken: ct);

                    return (user, permissionScope);
                }
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to get or create user: {permissionUserId}. Unhandled exception: {ex}");
            }

            return (null, permissionScope);
        }

        private async Task<IResourcePermissionResponse> GetOrCreateUsersPermissions(
            IEnumerable<(User user, IPermissionScope permissionScope)> usersWithPermisssionScope,
            string userId,
            CancellationToken ct)
        {
            var getOrCreateUserPermissionsTask = usersWithPermisssionScope
                .Where(tuple => !(tuple.user is null))
                .Select(tuple => GetOrCreateUserPermission(tuple.user, userId, tuple.permissionScope, ct));

            var permissions = await Task.WhenAll(getOrCreateUserPermissionsTask);

            return new ResourcePermissionResponse(permissions, userId, _endpointUrl);

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

                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                var partitionKeyValueJson = permissionResponse.Resource.ResourcePartitionKey.GetValueOrDefault().ToString();

                var partitionKeyValue = JArray.Parse(partitionKeyValueJson)?.FirstOrDefault()?.Value<string>();

                return new ResourcePermission(
                    permissionScope,
                    permissionResponse.Resource.Token,
                    permissionResponse.Resource.Id,
                    partitionKeyValue,
                    expiresUtc);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new ResourceTokenBrokerServiceException($"Unable to read or create user permissions. Unhandled exception: {ex}");
                }
            }

            return await CreateNewPermission(user, userId, permissionId, permissionScope, ct);
        }

        private async Task<IResourcePermission> CreateNewPermission(
            User user,
            string userId,
            string permissionId,
            IPermissionScope permissionScope,
            CancellationToken ct)
        {
            try
            {
                var container = _database.GetContainer(_collectionId);

                var partitionKeyValue = userId.ToPartitionKeyBy(permissionScope.PermissionMode);

                var partitionKey = new PartitionKey(permissionId);
                
                var permissionProperties = new PermissionProperties(
                    permissionId,
                    permissionScope.PermissionMode.ToCosmosPermissionMode(),
                    container,
                    partitionKey);

                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

                var permissionResponse = await user.CreatePermissionAsync(
                    permissionProperties,
                    Convert.ToInt32(_resourceTokenTtl.TotalSeconds),
                    cancellationToken: ct);
                
                if (!(permissionResponse?.Resource?.Token is null))
                {
                    return new ResourcePermission(
                        permissionScope, 
                        permissionResponse.Resource.Token, 
                        permissionResponse.Resource.Id,
                        partitionKeyValue,
                        expiresUtc);
                }

                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Status code: {permissionResponse?.StatusCode}");

            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Unhandled exception: {ex}");
            }
        }

        
        public ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            
            return new ValueTask();
        }
    }
}

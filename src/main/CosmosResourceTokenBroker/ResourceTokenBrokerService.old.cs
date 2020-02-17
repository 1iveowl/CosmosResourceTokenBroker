//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using CosmosResourceToken.Core.Broker;
//using CosmosResourceToken.Core.Model;
//using Microsoft.Azure.Cosmos;
//using MoreLinq;
//using Newtonsoft.Json.Linq;
//using static CosmosResourceToken.Core.Model.Constants;

//namespace CosmosResourceTokenBroker
//{
//    public class ResourceTokenBrokerService : IResourceTokenBrokerService
//    {
//        private readonly CosmosClient _cosmosClient;
//        private readonly Database _database;
//        private readonly string _collectionId;
//        private readonly string _endpointUrl;

//        private readonly TimeSpan _resourceTokenTtl;

//        public ResourceTokenBrokerService(
//            string endpointUrl, 
//            string key, 
//            string databaseId, 
//            string collectionId,
//            TimeSpan? resourceTokenTtl = default)
//        {
//            _collectionId = collectionId;
//            _endpointUrl = endpointUrl;

//            //var clientOptions = new CosmosClientOptions
//            //{
//            //    AllowBulkExecution = true
//            //};

//            _cosmosClient = new CosmosClient(endpointUrl, key);

//            _database = _cosmosClient.GetDatabase(databaseId);

//            if (resourceTokenTtl is null)
//            {
//                _resourceTokenTtl = TimeSpan.FromHours(1);
//            }
//        }

//        public async Task<IResourcePermissionResponse> Get(
//            string userId,
//            IEnumerable<IPermissionScope> permissionscopes, 
//            CancellationToken cancellationToken = default)
//        {
//            var user = await GetOrCreateCosmosUser(
//                userId, 
//                cancellationToken);

//            return await GetOrCreateUserPermissions(
//                user,
//                cancellationToken);
//        }

//        private async Task<User> GetOrCreateCosmosUser(string userid, CancellationToken ct)
//        {
//            try
//            {
//                var user = _database.GetUser(userid);

//                var userProperties = await user.ReadAsync(cancellationToken:ct);
                
//                return userProperties.User;
//            }
//            catch (CosmosException ex)
//            {
//                if (ex.StatusCode == HttpStatusCode.NotFound)
//                {
//                    return await _database.CreateUserAsync(userid, cancellationToken: ct);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ResourceTokenBrokerServiceException($"Unable to get user. Unhandled exception: {ex}");
//            }

//            return null;
//        }

//        private async Task<IResourcePermissionResponse> GetOrCreateUserPermissions(
//            User user,
//            CancellationToken ct)
//        {
//            var permissionIterator = user.GetPermissionQueryIterator<PermissionProperties>();

//            var existingPermissionsNested = new List<IEnumerable<PermissionProperties>>();

//            while (permissionIterator.HasMoreResults)
//            {
//                var permissions = await permissionIterator.ReadNextAsync(ct);

//                if (permissions?.Resource.Any() ?? false)
//                {
//                    existingPermissionsNested.Add(permissions?.Resource);
//                }
//            }

//            var existingPermissions = existingPermissionsNested.SelectMany(p => p)
//                .DistinctBy(p => p.Id);

//            var readPermissionTasks = KnownPermissionScopes
//                .Select(permissionScope => GetOrCreateUserPermission(user, permissionScope, existingPermissions, ct));
            
//            var resourcePermissions = await Task.WhenAll(readPermissionTasks);

//            return new ResourcePermissionResponse(resourcePermissions, user.Id, _endpointUrl);
//        }

//        private async Task<IResourcePermission> GetOrCreateUserPermission(
//            User user, 
//            IPermissionScope permissionScope,
//            IEnumerable<PermissionProperties> existingPermissions,
//            CancellationToken ct)
//        {
//            try
//            {
//                var permissionId = user.ToPermissionIdBy(permissionScope.Scope);

//                var existingPermission = existingPermissions.FirstOrDefault(p => p.Id == permissionId);

//                if (!(existingPermission is null))
//                {
//                    var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

//                    var partitionKeyValueJson = existingPermission.ResourcePartitionKey.GetValueOrDefault().ToString();

//                    var partitionKeyValue = JArray.Parse(partitionKeyValueJson)?.FirstOrDefault()?.Value<string>();

//                    return new ResourcePermission(
//                        permissionScope,
//                        existingPermission.Token,
//                        existingPermission.Id,
//                        partitionKeyValue,
//                        expiresUtc);
//                }
//            }
//            catch (CosmosException ex)
//            {
//                if (ex.StatusCode != HttpStatusCode.NotFound)
//                {
//                    throw new ResourceTokenBrokerServiceException($"Unable to read or create user permissions. Unhandled exception: {ex}");
//                }
//            }

//            return await CreateNewPermission(user, permissionScope, ct);
//        }

//        private async Task<IResourcePermission> CreateNewPermission(
//            User user,
//            IPermissionScope permissionScope,
//            CancellationToken ct)
//        {
//            try
//            {
//                var container = _database.GetContainer(_collectionId);

//                var partitionKeyValue = user.ToPartitionKeyBy(permissionScope.PermissionMode);

//                var partitionKey = new PartitionKey(partitionKeyValue);
                
//                var permissionProperties = new PermissionProperties(
//                    user.ToPermissionIdBy(permissionScope.Scope),
//                    permissionScope.PermissionMode.ToCosmosPermissionMode(),
//                    container,
//                    partitionKey);

//                var expiresUtc = DateTime.UtcNow + _resourceTokenTtl;

//                var permissionResponse = await user.CreatePermissionAsync(
//                    permissionProperties,
//                    Convert.ToInt32(_resourceTokenTtl.TotalSeconds),
//                    cancellationToken: ct);
                
//                if (!(permissionResponse?.Resource?.Token is null))
//                {
//                    return new ResourcePermission(
//                        permissionScope, 
//                        permissionResponse.Resource.Token, 
//                        permissionResponse.Resource.Id,
//                        partitionKeyValue,
//                        expiresUtc);
//                }

//                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Status code: {permissionResponse?.StatusCode}");

//            }
//            catch (Exception ex)
//            {
//                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Unhandled exception: {ex}");
//            }
//        }

        
//        public ValueTask DisposeAsync()
//        {
//            _cosmosClient?.Dispose();
            
//            return new ValueTask();
//        }
//    }


//}

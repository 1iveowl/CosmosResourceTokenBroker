using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosResourceTokenBroker
{
    public class BrokerService2 : IResourceTokenBrokerService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly string _collectionId;

        private readonly TimeSpan _resourceTokenTtl;

        private static string GetUserPartitionKey(string userId) => $"user-{userId}";
        private static string GetReadWriteUserPermission(string userId) => $"{userId}permission";

        public BrokerService2(string endpointUrl, string key, string databaseId, string collectionId, TimeSpan? resourceTokenTtl = default)
        {
            _cosmosClient = new CosmosClient(endpointUrl, key);
            _database = _cosmosClient.GetDatabase(databaseId);
            _collectionId = collectionId;

            if (resourceTokenTtl is null)
            {
                _resourceTokenTtl = TimeSpan.FromHours(1);
            }
        }

        public async Task<IPermissionToken> Get(string userId, CancellationToken cancellationToken = default)
        {
            var user = await GetOrCreateCosmosUser(userId, cancellationToken);

            return await GetOrCreateUserPermissions(
                user, 
                GetReadWriteUserPermission(userId), 
                cancellationToken);
        }

        private async Task<User> GetOrCreateCosmosUser(string userid, CancellationToken ct)
        {
            try
            {
                return _database.GetUser(userid);
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

        private async Task<IPermissionToken> GetOrCreateUserPermissions(User user, string permissionId, CancellationToken ct)
        {
            try
            {
                var p = user.GetPermissionQueryIterator<object>();

                var lst = new List<object>();

                while (p.HasMoreResults)
                {
                    lst.Add(await p.ReadNextAsync(ct));
                }

                var permission = user.GetPermission(permissionId);

                var expireIn = Convert.ToInt32(_resourceTokenTtl.TotalSeconds);

                var permissionResponse = await permission.ReadAsync(
                    tokenExpiryInSeconds: expireIn, 
                    cancellationToken:ct);

                if (permissionResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return await CreateNewPermission(user, permissionId, ct);
                }

                if (!(permissionResponse?.Resource?.Token is null))
                {
                    return CreatePermissionToken(permissionResponse, user);
                }
            }
            catch (Exception ex)
            {
                //if (ex.StatusCode != HttpStatusCode.NotFound)
                //{
                //    throw new ResourceTokenBrokerServiceException($"Unable to read or create user permissions. Unhandled exception: {ex}");
                //}
            }

            return await CreateNewPermission(user, permissionId, ct);
        }

        private async Task<IPermissionToken> CreateNewPermission(User user, string permissionId, CancellationToken ct)
        {
            var container = _database.GetContainer(_collectionId);

            var permissionProperties = new PermissionProperties(
                permissionId,
                PermissionMode.All,
                container,
                new PartitionKey(GetUserPartitionKey(user.Id)));

            try
            {
                var permissionResponse = await user.CreatePermissionAsync(permissionProperties, cancellationToken: ct);

                if (!(permissionResponse?.Resource?.Token is null))
                {
                    return CreatePermissionToken(permissionResponse, user);
                }

                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Status code: {permissionResponse?.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new ResourceTokenBrokerServiceException($"Unable to create new permission for user. Unhandled exception: {ex}");
            }

        }

        private IPermissionToken CreatePermissionToken(PermissionResponse permissionResponse, User user)
        {
            return new PermissionToken
            {
                Token = permissionResponse.Resource.Token,
                ExpiresUtc = DateTime.UtcNow + _resourceTokenTtl,
                UserId = user.Id,
                Id = permissionResponse.Resource.Id
            };
        }
        
        public ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            
            return new ValueTask();
        }
    }
}

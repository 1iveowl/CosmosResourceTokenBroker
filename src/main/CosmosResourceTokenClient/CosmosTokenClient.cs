using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using Microsoft.Azure.Cosmos;

namespace CosmosResourceTokenClient
{
    public class CosmosTokenClient : CosmosTokenClientHandler, ICosmosTokenClient
    {
        public CosmosTokenClient(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey<IResourcePermissionResponse> resourceTokenCache = null) : base(authService, resourceTokenBrokerUrl, resourceTokenCache)
        {

        }

        public async Task Create<T>(string id, T obj, DefaultPartitionKind defaultPartition) =>
            await Execute<T>(async resourcePermissionResponse =>
            {
                var container = GetContainer(resourcePermissionResponse, PermissionModeKind.UserReadWrite);
                
                return await Task.FromResult(default(T));
            }, PermissionModeKind.UserReadWrite);
        

        public async Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition) =>
            await Execute<T>(async resourcePermissionResponse =>
            {
                var container = GetContainer(resourcePermissionResponse, PermissionModeKind.UserRead);

                container.ReadItemStreamAsync()
                
                return await Task.FromResult(default(T));
            }, PermissionModeKind.UserRead);

        public Task Replace<T>(string id, T obj, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task Delete<T>(string id, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        private static Container GetContainer(IResourcePermissionResponse resourcePermissionResponse, PermissionModeKind permissionMode)
        {
            var permission = resourcePermissionResponse.ResourcePermissions.FirstOrDefault(p =>
                p.PermissionMode == permissionMode);

            using var cosmosClient = new CosmosClient(resourcePermissionResponse.EndpointUrl, permission?.ResourceToken);

            return cosmosClient.GetDatabase(resourcePermissionResponse.DatabaseId).GetContainer(resourcePermissionResponse.CollectionId);
        }


    }
}

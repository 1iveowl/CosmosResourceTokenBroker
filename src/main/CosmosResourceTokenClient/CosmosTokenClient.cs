using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceTokenClient
{
    public class CosmosTokenClient : ICosmosTokenClient
    {
        private readonly CosmosTokenClientHandler _cosmosClientHandler;

        public CosmosTokenClient(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey<IResourcePermissionResponse> resourceTokenCache = null)
        {
            _cosmosClientHandler = new CosmosTokenClientHandler(authService, resourceTokenBrokerUrl, resourceTokenCache);
        }

        public async Task Create<T>(T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken)=>
            await _cosmosClientHandler.Execute<T>(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Global)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientWrapper(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                return await cosmosClientEx.Create(item, cancellationToken);

            }, PermissionModeKind.UserReadWrite);
        

        public async Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken) =>
            await _cosmosClientHandler.Execute<T>(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;

                await using var cosmosClientEx = new CosmosClientWrapper(resourcePermissionResponse, PermissionModeKind.UserRead);

                return await cosmosClientEx.Read<T>(id, cancellationToken);
            }, PermissionModeKind.UserRead);

        public async Task Replace<T>(T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken) =>
            await _cosmosClientHandler.Execute<T>(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Global)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                return await Task.FromResult(default(T));
            }, PermissionModeKind.UserReadWrite);


        public async Task Delete<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken) =>
            await _cosmosClientHandler.Execute<T>(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Global)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                return await Task.FromResult(default(T));
            }, PermissionModeKind.UserReadWrite);

        public async Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken)=>
            await _cosmosClientHandler.Execute<T>(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;


                return await Task.FromResult(new List<T>());
            }, PermissionModeKind.UserReadWrite);
    }
}

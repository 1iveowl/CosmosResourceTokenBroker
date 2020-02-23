using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceTokenClient
{
    public class CosmosTokenClient : ICosmosTokenClient, IAsyncDisposable
    {
        private readonly CosmosTokenClientHandler _cosmosClientHandler;

        public CosmosTokenClient(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey resourceTokenCache = null)
        {
            _cosmosClientHandler = new CosmosTokenClientHandler(authService, resourceTokenBrokerUrl, resourceTokenCache);
        }

        public async Task Create<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientWrapper<T>(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                return await cosmosClientEx.Create(id, item, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        public async Task Replace<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientWrapper<T>(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                return await cosmosClientEx.Replace(id, item, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        public async Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;

                await using var cosmosClientEx = new CosmosClientWrapper<T>(resourcePermissionResponse, permissionMode);

                return await cosmosClientEx.Read(id, cancellationToken);
            }, PermissionModeKind.UserRead, cancellationToken);


        public async Task Delete<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read-only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientWrapper<T>(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                await cosmosClientEx.Delete(id, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        public async Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;

                await using var cosmosClientEx = new CosmosClientWrapper<T>(resourcePermissionResponse, permissionMode);

                return await cosmosClientEx.GetList(cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        public async ValueTask DisposeAsync()
        {
            await _cosmosClientHandler.DisposeAsync();
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using Microsoft.Azure.Cosmos;

namespace CosmosResourceTokenClient
{
    internal class CosmosClientWrapper : IAsyncDisposable
    {
        private readonly CosmosClient _cosmosClient;

        private readonly Container _container;

        private readonly PartitionKey _partitionKey;

        internal CosmosClientWrapper(IResourcePermissionResponse resourcePermissionResponse, PermissionModeKind permissionMode)
        {
            var currentPermission = resourcePermissionResponse?.ResourcePermissions?
                .FirstOrDefault(p => p?.PermissionMode == permissionMode);

            _partitionKey = new PartitionKey(currentPermission?.PartitionKey);

            _cosmosClient = new CosmosClient(resourcePermissionResponse?.EndpointUrl, currentPermission?.ResourceToken);

            _container = _cosmosClient
                .GetDatabase(resourcePermissionResponse?.DatabaseId)
                .GetContainer(resourcePermissionResponse?.CollectionId);
        }

        internal async Task<T> Create<T>(T item, CancellationToken ct)
        {
            try
            {
                var itemResponse = await _container.CreateItemAsync(item, _partitionKey, cancellationToken: ct);

                if (itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    return itemResponse.Resource;
                }

                throw new CosmosClientException($"Unable to create: {typeof(T).FullName}. Status Code: {itemResponse.StatusCode.ToString()}");
                
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new CosmosClientException($"Unable to create document. Id already exist. Change id, delete first or try replace instead.", ex);
                }

                throw new CosmosClientException($"Unable to create: {typeof(T).FullName}. Status Code: {ex.StatusCode}.", ex);
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to create: {typeof(T).FullName}.", ex);
            }
        }

        internal async Task<T> Replace<T>(T item, CancellationToken ct)
        {
            try
            {
                var itemResponse = await _container.UpsertItemAsync(item, _partitionKey, cancellationToken: ct);

                if (itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    return itemResponse.Resource;
                }

                throw new CosmosClientException($"Unable to replace/Upsert: {typeof(T).FullName}. Status Code: {itemResponse.StatusCode.ToString()}");
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to Replace/Upsert: {typeof(T).FullName}.", ex);
            }
        }
        internal async Task<T> Read<T>(string id, CancellationToken ct)
        {
            try
            {
                var itemResponse = await _container.ReadItemAsync<T>(id, _partitionKey, cancellationToken: ct);

                if (itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    return itemResponse.Resource;
                }

                throw new CosmosClientException($"Unable to read: {typeof(T).FullName}. Status Code: {itemResponse.StatusCode.ToString()}");
            }
            catch (CosmosException ex)
            {
                throw new CosmosClientException($"Unable to read: {typeof(T).FullName}. Status Code: {ex.StatusCode}.", ex);
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to read: {typeof(T).FullName}.", ex);
            }

        }


        public async ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            await Task.CompletedTask;
        }
    }
}

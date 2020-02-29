using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using CosmosResourceTokenClient.JsonSerialize;
using CosmosResourceTokenClient.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;

namespace CosmosResourceTokenClient
{
    internal class CosmosClientWrapper<T>: IAsyncDisposable
    {
        private readonly CosmosClient _cosmosClient;

        private readonly Container _container;

        private readonly PartitionKey _partitionKey;
        private readonly string _partitionKeyStr;

        internal CosmosClientWrapper(
            IResourcePermissionResponse resourcePermissionResponse, 
            PermissionModeKind permissionMode)
        {
            var currentPermission = resourcePermissionResponse?.ResourcePermissions?
                .FirstOrDefault(p => p?.PermissionMode == permissionMode);

            _partitionKey = new PartitionKey(currentPermission?.PartitionKey);
            _partitionKeyStr = currentPermission?.PartitionKey;

            var customJsonSerializer = new JsonSerializerSettings
            {
                ContractResolver = new PartitionKeyContractResolver<T>(resourcePermissionResponse?.PartitionKeyHeader)
            };

            _cosmosClient = new CosmosClientBuilder(resourcePermissionResponse?.EndpointUrl, currentPermission?.ResourceToken)
                .WithCustomSerializer(new CosmosJsonNetSerializer(customJsonSerializer))
                .Build();

            _container = _cosmosClient
                .GetDatabase(resourcePermissionResponse?.DatabaseId)
                .GetContainer(resourcePermissionResponse?.CollectionId);
        }

        internal async Task<T> Create(string id, T item, CancellationToken ct)
        {
            try
            {
                var cosmosItem = new CosmosItem<T>(item, id, _partitionKeyStr);

                var itemResponse = await _container.CreateItemAsync(cosmosItem, _partitionKey, cancellationToken: ct).ConfigureAwait(false);

                if (itemResponse.StatusCode == HttpStatusCode.Created)
                {
                    return itemResponse.Resource.Document;
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

        internal async Task<T> Replace(string id, T item, CancellationToken ct)
        {
            try
            {
                var cosmosItem = new CosmosItem<T>(item, id, _partitionKeyStr);

                var itemResponse = await _container.UpsertItemAsync(cosmosItem, _partitionKey, cancellationToken: ct).ConfigureAwait(false);

                if (itemResponse.StatusCode == HttpStatusCode.Created || itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    return itemResponse.Resource.Document;
                }

                throw new CosmosClientException($"Unable to replace/Upsert: {typeof(T).FullName}. Status Code: {itemResponse.StatusCode.ToString()}");
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to Replace/Upsert: {typeof(T).FullName}.", ex);
            }
        }
        internal async Task<T> Read(string id, CancellationToken ct)
        {
            try
            {
                var itemResponse = await _container.ReadItemAsync<CosmosItem<T>>(id, _partitionKey, cancellationToken: ct).ConfigureAwait(false);

                if (itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    return itemResponse.Resource.Document;
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

        internal async Task Delete(string id, CancellationToken ct)
        {
            try
            {
                var itemResponse = await _container.DeleteItemAsync<CosmosItem<T>>(id, _partitionKey, cancellationToken: ct).ConfigureAwait(false);

                if (itemResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return;
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
        
        internal async Task<IEnumerable<object>> GetPartitionDocuments(CancellationToken ct)
        {
            try
            {
                var queryRequestOption = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(_partitionKeyStr)
                };

                var setIterator = _container
                    .GetItemLinqQueryable<CosmosItem<object>>(true, requestOptions: queryRequestOption)
                    .ToFeedIterator();

                var itemList = new List<object>();
                
                while (setIterator.HasMoreResults)
                {
                    foreach (var cosmosItem in await setIterator.ReadNextAsync(cancellationToken:ct).ConfigureAwait(false))
                    {
                        itemList.Add(cosmosItem.Document);
                    }
                }

                return itemList;

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

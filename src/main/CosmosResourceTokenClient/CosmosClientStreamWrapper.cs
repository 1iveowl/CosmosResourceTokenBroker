using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using CosmosResourceTokenClient.Model;
using Microsoft.Azure.Cosmos;

namespace CosmosResourceTokenClient
{
    [Preserve(AllMembers = true)]
    internal class CosmosClientStreamWrapper : IAsyncDisposable
    {
        private readonly CosmosClient _cosmosClient;

        private readonly Container _container;

        private readonly PartitionKey _partitionKey;
        private readonly string _partitionKeyStr;
        private readonly string _partitionKeyHeader;

        internal CosmosClientStreamWrapper(
            IResourcePermissionResponse resourcePermissionResponse,
            PermissionModeKind permissionMode)
        {
            var currentPermission = resourcePermissionResponse?.ResourcePermissions?
                .FirstOrDefault(p => p?.PermissionMode == permissionMode);

            _partitionKey = new PartitionKey(currentPermission?.PartitionKey);
            _partitionKeyStr = currentPermission?.PartitionKey;
            _partitionKeyHeader = resourcePermissionResponse?.PartitionKeyHeader;

            try
            {
                _cosmosClient = new CosmosClient(resourcePermissionResponse?.EndpointUrl, currentPermission?.ResourceToken);

                _container = _cosmosClient
                    .GetDatabase(resourcePermissionResponse?.DatabaseId)
                    .GetContainer(resourcePermissionResponse?.CollectionId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to instantiate {nameof(CosmosClientStreamWrapper)}. Unhandled exception {ex}");
                throw new CosmosClientException($"Unable to instantiate {nameof(CosmosClientStreamWrapper)}. Unhandled exception {ex}", ex);
            }
        }

        internal async Task Create<T>(string id, T item, CancellationToken ct)
        {
            try
            {
                await using var cosmosItem = new CosmosItem<T>(item, id);

                var payload = await cosmosItem.ToStream(_partitionKeyHeader, _partitionKeyStr, ct);

                using var response =  await _container.CreateItemStreamAsync(payload, _partitionKey, cancellationToken: ct);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                throw new CosmosClientException(
                    $"Unable to create with stream payload. Status Code: {response.StatusCode.ToString()}");
            }
            catch (ConfigurationException ex)
            {
                // There's an issue with Android and the System.Configuration.ConfigurationManager: https://github.com/xamarin/Xamarin.Forms/issues/5935
                // For now we are ignoring ConfigurationExceptions as it seems that the operation works despite the exception.
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to create with stream payload. Unhandled exception: {ex}", ex);
            }
        }

        internal async Task Replace<T>(string id, T item, CancellationToken ct)
        {
            try
            {
                await using var cosmosItem = new CosmosItem<T>(item, id, _partitionKeyStr);

                var payload = await cosmosItem.ToStream(_partitionKeyHeader, _partitionKeyStr, ct);

                using var response =
                    await _container.UpsertItemStreamAsync(payload, _partitionKey, cancellationToken: ct);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                throw new CosmosClientException(
                    $"Unable to replace/upsert: {typeof(T).FullName} with id: {id}. Status Code: {response.StatusCode.ToString()}");
            }
            catch (ConfigurationException ex)
            {
                // There's an issue with Android and the System.Configuration.ConfigurationManager: https://github.com/xamarin/Xamarin.Forms/issues/5935
                // For now we are ignoring ConfigurationExceptions as it seems that the operation works despite the exception.
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to replace/upsert: {typeof(T).FullName} with id: {id}. Unhandled exception: {ex}", ex);
            }
        }

        internal async Task<T> Read<T>(string id, CancellationToken ct)
        {
            ResponseMessage response;

            try
            {
                response = await _container.ReadItemStreamAsync(id, _partitionKey, cancellationToken:ct);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                throw new CosmosClientException($"Unable to read: {id} to Stream", ex);
            }

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    await using var cosmosItem = new CosmosItem<T>();

                    var item = await cosmosItem.GetItemFromStream(response.Content, ct);

                    return item;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                throw new CosmosClientException($"Unable deserialize document with '{id}' from Stream.", ex);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new DataException($"Document with id '{id}' not found");
            }

            throw new CosmosClientException($"Unable to read: {id} to Stream. Status code: {response.StatusCode.ToString()}");

        }

        internal async Task<IEnumerable<string>> GetPartitionDocuments(CancellationToken ct)
        {
            try
            {
                var queryRequestOption = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(_partitionKeyStr)
                };

                var feedIterator = _container
                    .GetItemQueryStreamIterator(requestOptions: queryRequestOption);

                var jsonItemList = new List<IEnumerable<string>>();

                while (feedIterator.HasMoreResults)
                {
                    using var response = await feedIterator.ReadNextAsync(cancellationToken: ct);

                    await using var cosmosItem = new CosmosItem<string>();

                    var jsonStrings = await cosmosItem.GetJsonStringsFromStream(response.Content, ct);

                    if (jsonStrings?.Any() ?? false)
                    {
                        jsonItemList.Add(jsonStrings);
                    }
                }

                return jsonItemList.SelectMany(d => d);
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to read partition: {_partitionKeyStr}.", ex);
            }
        }

        internal async Task<IEnumerable<T>> GetPartitionObjects<T>(CancellationToken ct)
        {
            try
            {
                var queryRequestOption = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(_partitionKeyStr)
                };

                var feedIterator = _container
                    .GetItemQueryStreamIterator(requestOptions: queryRequestOption);

                var jsonItemList = new List<IEnumerable<T>>();

                while (feedIterator.HasMoreResults)
                {
                    using var response = await feedIterator.ReadNextAsync(cancellationToken: ct);

                    await using var cosmosItem = new CosmosItem<T>();

                    var items = await cosmosItem.GetItemsFromStream(response.Content, ct);

                    if (items?.Any() ?? false)
                    {
                        jsonItemList.Add(items);
                    }
                }

                return jsonItemList.SelectMany(d => d);
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to read partition: {_partitionKeyStr}.", ex);
            }
        }

        internal async Task Delete(string id, CancellationToken ct)
        {
            try
            {
                using var response = await _container.DeleteItemStreamAsync(id, _partitionKey, cancellationToken: ct);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (ConfigurationException)
            {
                // There's an issue with Android and the System.Configuration.ConfigurationManager: https://github.com/xamarin/Xamarin.Forms/issues/5935
                // For now we are ignoring PlatformNotSupportedExceptions as it seems that the operation works despite the exception.
            }
            catch (Exception ex)
            {
                throw new CosmosClientException($"Unable to delete: {id} to Stream", ex);
            }

            throw new CosmosClientException($"Unable to delete: {id} to Stream");
        }

        public async ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            await Task.CompletedTask;
        }
    }
}

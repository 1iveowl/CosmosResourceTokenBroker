using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceTokenClient.JsonSerialize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosResourceTokenClient.Model
{
    [Preserve(AllMembers = true)]
    public class CosmosItem<T> : ICosmosItem<T>, IAsyncDisposable
    {
        private const string DocumentPropertyName = "document";
        private const string IdPropertyName = "id";

        private readonly Stream _memoryStream;

        [JsonPropertyName(DocumentPropertyName)]
        [JsonProperty(PropertyName = DocumentPropertyName)]
        public T Document { get; set; }

        [JsonPropertyName(IdPropertyName)]
        [JsonProperty(PropertyName = IdPropertyName)]
        public string Id { get; set; }

        // ReSharper disable once InconsistentNaming
        public string REPLACE_WITH_PARTITION_KEY_HEADER { get; set; }

        public CosmosItem()
        {
        }

        internal CosmosItem(T document, string id, string partitionKey) : this(document, id)
        {
            REPLACE_WITH_PARTITION_KEY_HEADER = partitionKey;
        }

        internal CosmosItem(T document, string id)
        {
            Document = document;
            Id = id;

            _memoryStream = new MemoryStream();
        }
        
        public virtual async Task<Stream> ToStream(string partitionKeyHeader, string partitionKey, CancellationToken ct = default)
        {
            // Using NewtonSoft Json.NET here to utilize the ContractResolver option.
            var customJsonSerializer = new JsonSerializerSettings
            {
                ContractResolver = new PartitionKeyContractResolver<T>(partitionKeyHeader, nameof(REPLACE_WITH_PARTITION_KEY_HEADER))
            };

            REPLACE_WITH_PARTITION_KEY_HEADER = partitionKey;

            var cosmosItemJson = JsonConvert.SerializeObject(this, Formatting.None, customJsonSerializer);

            await _memoryStream.WriteAsync(Encoding.UTF8.GetBytes(cosmosItemJson), ct);

            return _memoryStream;
        }

        public virtual async Task<ICosmosItem<T>> GetObjectFromStream(Stream stream, CancellationToken ct = default)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(sr);

            await Task.CompletedTask;

            return serializer.Deserialize<CosmosItem<T>>(jsonReader);
        }

        public virtual async Task<IEnumerable<string>> GetJsonStringsFromStream(Stream stream, CancellationToken ct = default)
        {
            if (stream is MemoryStream memoryStream)
            {
                var itemAsJsonStr = Encoding.UTF8.GetString(memoryStream.ToArray());

                var jObj = JObject.Parse(itemAsJsonStr);

                await Task.CompletedTask;

                return jObj["Documents"]?
                    .Select(doc => doc?[DocumentPropertyName]?.ToString())
                    .Where(doc => !string.IsNullOrEmpty(doc));
            }

            return new List<string>();
        }

        public async ValueTask DisposeAsync()
        {
            if (_memoryStream is null)
            {
                return;
            }

            await _memoryStream.DisposeAsync();
        }
    }
}

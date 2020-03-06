using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceTokenClient.JsonSerialize;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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

        public virtual async Task<T> GetItemFromStream(Stream stream, CancellationToken ct = default)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(sr);

            await Task.CompletedTask;

            return serializer.Deserialize<CosmosItem<T>>(jsonReader).Document;
        }

        public async Task<IEnumerable<T>> GetItemsFromStream(Stream stream, CancellationToken ct = default)
        {
            var jsonStrList = await GetJsonStringsFromStream(stream, ct);

            return jsonStrList.Select(jsonStr => System.Text.Json.JsonSerializer.Deserialize<T>(jsonStr.ToString()));
        }

        public virtual async Task<IEnumerable<string>> GetJsonStringsFromStream(Stream stream, CancellationToken ct = default)
        {

            var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var documents = jsonDocument.RootElement
                .EnumerateObject()
                .FirstOrDefault(x => x.NameEquals("Documents"));

            if (documents.Value.ValueKind == JsonValueKind.Array)
            {
                var serializer = new JsonSerializer();

                var list = documents.Value.EnumerateArray()
                    .Select(o => o.EnumerateObject()
                        .FirstOrDefault(p => p.NameEquals("document")).Value)
                    .Select(x => x.ToString());

                return list;
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

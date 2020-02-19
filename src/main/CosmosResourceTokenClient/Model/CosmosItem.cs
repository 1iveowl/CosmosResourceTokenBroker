
using CosmosResourceToken.Core;
using Newtonsoft.Json;

namespace CosmosResourceTokenClient.Model
{
    [Preserve(AllMembers = true)]
    public class CosmosItem<T>
    {
        [JsonProperty(PropertyName = "document")]
        public T Document { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string PartitionKeyHeaderNameForCosmosItem { get; set; }

        public CosmosItem() { }

        public CosmosItem(T document, string id, string partitionKey)
        {
            Document = document;
            Id = id;
            PartitionKeyHeaderNameForCosmosItem = partitionKey;
        }
    }
}

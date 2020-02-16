using Newtonsoft.Json;

namespace CosmosResourceTokenBroker
{
    public class InitObject
    {
        [JsonProperty(PropertyName = "id")]
        private string Id { get; }

        [JsonProperty(PropertyName = "bmaas")]
        public string Partition { get; set; }

        public InitObject(string id, string partitionKey)
        {
            Id = id;
            Partition = partitionKey;
        }
    }
}

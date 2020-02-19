using System.Collections.Generic;
using CosmosResourceToken.Core.Serializer;
using Newtonsoft.Json;

namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public class ResourcePermissionResponse : IResourcePermissionResponse
    {

        [JsonProperty(PropertyName = "permission")]
        //[JsonConverter(typeof(ConcreteTypeConverter<List<ResourcePermission>>))]
        public IEnumerable<IResourcePermission> ResourcePermissions { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
        
        [JsonProperty(PropertyName = "endpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty(PropertyName = "databaseId")]
        public string DatabaseId { get; set; }

        [JsonProperty(PropertyName = "collectionId")]
        public string CollectionId { get; set; }

        [JsonProperty(PropertyName = "partitionKeyHeader")]
        public string PartitionKeyHeader { get; set; }

        public ResourcePermissionResponse() { }

        public ResourcePermissionResponse(
            IEnumerable<IResourcePermission> permissions,
            string userId,
            string endpointUrl, 
            string databaseId,
            string collectionId,
            string partitionKeyHeader)
        {
            ResourcePermissions = permissions;
 
            UserId = userId;
            EndpointUrl = endpointUrl;
            DatabaseId = databaseId;
            CollectionId = collectionId;
            PartitionKeyHeader = partitionKeyHeader;
        }
    }
}

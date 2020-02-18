using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public class ResourcePermissionResponse : IResourcePermissionResponse
    {

        [JsonProperty(PropertyName = "permission")]
        public IEnumerable<IResourcePermission> ResourcePermissions { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
        
        [JsonProperty(PropertyName = "endpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty(PropertyName = "databaseId")]
        public string DatabaseId { get; }

        [JsonProperty(PropertyName = "collectionId")]
        public string CollectionId { get; }

        public ResourcePermissionResponse() { }

        public ResourcePermissionResponse(
            IEnumerable<IResourcePermission> permissions,
            string userId,
            string endpointUrl, 
            string databaseId,
            string collectionId)
        {
            ResourcePermissions = permissions;
 
            UserId = userId;
            EndpointUrl = endpointUrl;
            DatabaseId = databaseId;
            CollectionId = collectionId;
        }
    }
}

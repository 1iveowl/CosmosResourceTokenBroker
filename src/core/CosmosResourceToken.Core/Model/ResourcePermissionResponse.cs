using System.Collections.Generic;
using Newtonsoft.Json;

namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Resource Permission Response.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ResourcePermissionResponse : IResourcePermissionResponse
    {
        /// <summary>
        ///     <para>
        ///         List of resource permissions.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "resourcePermissions")]
        public IEnumerable<IResourcePermission> ResourcePermissions { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set user id.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set Azure Cosmos DB end-point.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "endpointUrl")]
        public string EndpointUrl { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set Azure Cosmos DB database id.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "databaseId")]
        public string DatabaseId { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set Azure Cosmos DB collection id.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "collectionId")]
        public string CollectionId { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set Azure Cosmos DB collection partition key header.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "partitionKeyHeader")]
        public string PartitionKeyHeader { get; set; }

        /// <summary>
        ///     <para>
        ///         Instance of resource permission response.
        ///     </para>
        /// </summary>
        public ResourcePermissionResponse() { }

        /// <summary>
        ///     <para>
        ///         Instance of resource permission response.
        ///     </para>
        /// <param name="permissions">Resource permission.</param>
        /// <param name="userId">User id (guid)</param>
        /// <param name="endpointUrl">Azure Cosmos DB end-point.</param>
        /// <param name="databaseId">Azure Cosmos DB database id.</param>
        /// <param name="collectionId">Azure Cosmos DB collection id.</param>
        /// <param name="partitionKeyHeader">Azure Cosmos DB collection partition key header.</param>
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

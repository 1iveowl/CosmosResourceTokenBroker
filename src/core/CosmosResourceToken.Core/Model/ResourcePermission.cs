using System;
using Newtonsoft.Json;

namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Resource permission.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ResourcePermission : IResourcePermission
    {
        /// <summary>
        ///     <para>
        ///         Get and set permission mode.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "permissionMode")]
        public PermissionModeKind PermissionMode { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set permission scope.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set resource token.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "resourceToken")]
        public string ResourceToken { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set resource permission id.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set permission expiration (UTC).
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "expires")]
        public DateTime ExpiresUtc { get; set; }

        /// <summary>
        ///     <para>
        ///         Get and set partition key.
        ///     </para>
        /// </summary>
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        ///     <para>
        ///         Instance of resource permission.
        ///     </para>
        /// </summary>
        public ResourcePermission() { }


        /// <summary>
        ///     <para>
        ///         Instance of resource permission
        ///     </para>
        /// </summary>
        /// <param name="permissionScope">Permission scope.</param>
        /// <param name="resourceToken">Resource token.</param>
        /// <param name="id">Permission id.</param>
        /// <param name="partitionKey">Partition key</param>
        /// <param name="expiresUtc">Permission expiration (UTC)</param>
        public ResourcePermission(
            IPermissionScope permissionScope, 
            string resourceToken,
            string id,
            string partitionKey,
            DateTime expiresUtc) : this(permissionScope.PermissionMode, permissionScope.Scope, resourceToken, id, partitionKey, expiresUtc) { }

        /// <summary>
        ///     <para>
        ///         Instance of resource permission
        ///     </para>
        /// </summary>
        /// <param name="permissionMode">Permission mode.</param>
        /// <param name="scope">Permission scope.</param>
        /// <param name="resourceToken">Resource token.</param>
        /// <param name="id">Permission id.</param>
        /// <param name="partitionKey">Partition key.</param>
        /// <param name="expiresUtc">Permission expiration (UTC)</param>
        public ResourcePermission(
            PermissionModeKind permissionMode,
            string scope,
            string resourceToken,
            string id,
            string partitionKey,
            DateTime expiresUtc)
        {
            Scope = scope;
            PermissionMode = permissionMode;
            ResourceToken = resourceToken;
            Id = id;
            PartitionKey = partitionKey;
            ExpiresUtc = expiresUtc;
        }
    }
}

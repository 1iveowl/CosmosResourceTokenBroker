using System;
using Newtonsoft.Json;

namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public class ResourcePermission : IResourcePermission
    {
        [JsonProperty(PropertyName = "permissionMode")]
        public PermissionModeKind PermissionMode { get; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "resourceToken")]
        public string ResourceToken { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "expires")]
        public DateTime ExpiresUtc { get; set; }

        public ResourcePermission() { }

        public ResourcePermission(
            IPermissionScope permissionScope, 
            string resourceToken,
            string id,
            DateTime expiresUtc) : this(permissionScope.PermissionMode, permissionScope.Scope, resourceToken, id, expiresUtc) { }

        public ResourcePermission(
            PermissionModeKind permissionMode,
            string scope,
            string resourceToken,
            string id,
            DateTime expiresUtc)
        {
            Scope = scope;
            PermissionMode = permissionMode;
            ResourceToken = resourceToken;
            Id = id;
            ExpiresUtc = expiresUtc;
        }
    }
}

using System;

namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public interface IResourcePermission
    {
        PermissionModeKind PermissionMode { get; }

        string Scope { get; }

        string ResourceToken { get; }

        string Id { get; }

        DateTime ExpiresUtc { get; }

        string PartitionKey { get; }
    }
}

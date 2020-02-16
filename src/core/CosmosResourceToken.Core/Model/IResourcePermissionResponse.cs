using System;
using System.Collections.Generic;

namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public interface IResourcePermissionResponse
    {
        IEnumerable<IResourcePermission> ResourcePermissions { get; }
        string UserId { get; }
        string EndpointUrl { get; }
    }
}

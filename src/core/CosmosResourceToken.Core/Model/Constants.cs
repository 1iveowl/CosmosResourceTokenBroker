using System.Collections.Generic;

namespace CosmosResourceToken.Core.Model
{
    public static class Constants
    {
        public const string PermissionScopePrefix = ".permission.";
        public const string UserReadOnlyScope = "user.readonly";
        public const string UserReadWriteScope = "user.readwrite";
        public const string SharedReadOnlyScope = "shared.readonly";

        public static readonly IEnumerable<IPermissionScope> KnownPermissionScopes = new List<IPermissionScope>
        {
            new PermissionScope(PermissionModeKind.UserRead, UserReadOnlyScope),
            new PermissionScope(PermissionModeKind.UserReadWrite, UserReadWriteScope),
            new PermissionScope(PermissionModeKind.SharedRead, SharedReadOnlyScope)
        };
    }
}

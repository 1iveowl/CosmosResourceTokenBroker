namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public class PermissionScope : IPermissionScope
    {
        public PermissionModeKind PermissionMode { get; }
        public string Scope { get; }

        public PermissionScope(PermissionModeKind permissionMode, string scope)
        {
            PermissionMode = permissionMode;
            Scope = scope;
        }
    }
}

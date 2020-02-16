namespace CosmosResourceToken.Core.Model
{
    [Preserve(AllMembers = true)]
    public interface IPermissionScope
    {
        PermissionModeKind PermissionMode { get; }
        string Scope { get; }
    }
}

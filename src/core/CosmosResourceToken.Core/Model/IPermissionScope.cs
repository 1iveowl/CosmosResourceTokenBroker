namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Interface for Permission scope.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IPermissionScope
    {
        /// <summary>
        ///     <para>
        ///         Get Permission Mode.
        ///     </para>
        /// </summary>
        PermissionModeKind PermissionMode { get; }

        /// <summary>
        ///     <para>
        ///         Get scope.
        ///     </para>
        /// </summary>
        string Scope { get; }
    }
}

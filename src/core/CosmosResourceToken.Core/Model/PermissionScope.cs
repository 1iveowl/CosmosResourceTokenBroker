namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Permission Scope.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class PermissionScope : IPermissionScope
    {
        /// <summary>
        ///     <para>
        ///         Get permission mode.
        ///     </para>
        /// </summary>
        public PermissionModeKind PermissionMode { get; }

        /// <summary>
        ///     <para>
        ///         Get scope.
        ///     </para>
        /// </summary>
        public string Scope { get; }

        /// <summary>
        ///     <para>
        ///         Instance of permission scope.
        ///     </para>
        /// </summary>
        /// <param name="permissionMode">Permission mode enum.</param>
        /// <param name="scope">Permission scope.</param>
        public PermissionScope(PermissionModeKind permissionMode, string scope)
        {
            PermissionMode = permissionMode;
            Scope = scope;
        }
    }
}

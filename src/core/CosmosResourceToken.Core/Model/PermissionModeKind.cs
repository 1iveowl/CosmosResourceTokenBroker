namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Permission mode enum.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum PermissionModeKind
    {
        /// <summary>
        ///     <para>
        ///         User read-only permission.
        ///     </para>
        /// </summary>
        UserRead,

        /// <summary>
        ///     <para>
        ///         User read and write permission.
        ///     </para>
        /// </summary>
        UserReadWrite,

        /// <summary>
        ///     <para>
        ///         Shared read-only permission.
        ///     </para>
        /// </summary>
        SharedRead
    }
}

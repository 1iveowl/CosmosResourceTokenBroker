namespace CosmosResourceToken.Core.Client
{
    /// <summary>
    ///     <para>
    ///         Default partition enum.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum DefaultPartitionKind
    {
        /// <summary>
        ///     <para>
        ///         User document.
        ///     </para>
        /// </summary>
        UserDocument,

        /// <summary>
        ///     <para>
        ///         Shared document.
        ///     </para>
        /// </summary>
        Shared
    }
}

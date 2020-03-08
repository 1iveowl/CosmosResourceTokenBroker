using System;

namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Interface for Resource Permission 
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IResourcePermission
    {
        /// <summary>
        ///     <para>
        ///         Get PermissionMode.
        ///     </para>
        /// </summary>
        PermissionModeKind PermissionMode { get; }

        /// <summary>
        ///     <para>
        ///         Get scope.
        ///     </para>
        /// </summary>
        string Scope { get; }

        /// <summary>
        ///     <para>
        ///         Get Resource Token.
        ///     </para>
        /// </summary>

        string ResourceToken { get; }

        /// <summary>
        ///     <para>
        ///         Get Permission id.
        ///     </para>
        /// </summary>
        string Id { get; }


        /// <summary>
        ///     <para>
        ///         Get permission Expiration (UTC)
        ///     </para>
        /// </summary>
        DateTime ExpiresUtc { get; }

        /// <summary>
        ///     <para>
        ///         Get partition ksey.
        ///     </para>
        /// </summary>
        string PartitionKey { get; }
    }
}

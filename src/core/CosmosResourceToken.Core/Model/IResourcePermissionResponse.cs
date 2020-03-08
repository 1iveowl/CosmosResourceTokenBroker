using System;
using System.Collections.Generic;

namespace CosmosResourceToken.Core.Model
{
    /// <summary>
    ///     <para>
    ///         Interface for resource permission response.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IResourcePermissionResponse
    {
        /// <summary>
        ///     <para>
        ///         List of resource permissions.
        ///     </para>
        /// </summary>
        IEnumerable<IResourcePermission> ResourcePermissions { get; }

        /// <summary>
        ///     <para>
        ///         Get user id
        ///     </para>
        /// </summary>
        string UserId { get; }

        /// <summary>
        ///     <para>
        ///         Get Azure Cosmos DB end-point.
        ///     </para>
        /// </summary>
        string EndpointUrl { get; }

        /// <summary>
        ///     <para>
        ///         Get Azure Cosmos DB database id.
        ///     </para>
        /// </summary>
        string DatabaseId { get; }

        /// <summary>
        ///     <para>
        ///         Get Azure Cosmos DB collection id.
        ///     </para>
        /// </summary>
        string CollectionId { get; }

        /// <summary>
        ///     <para>
        ///         Get Azure Cosmos DB collection partition key header.
        ///     </para>
        /// </summary>
        string PartitionKeyHeader { get; }

    }
}

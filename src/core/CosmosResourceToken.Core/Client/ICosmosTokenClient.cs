using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    /// <summary>
    ///     <para>
    ///         Interface for the Cosmos token client. 
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface ICosmosTokenClient
    {
        /// <summary>
        ///     <para>
        ///         Create an Azure Cosmos DB entity with a document payload of type <c>T</c>.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document payload.</typeparam>
        /// <param name="id">Id of entity.</param>
        /// <param name="item">Object of type T to be stored as the payload of the Cosmos DB entity.</param>
        /// <param name="defaultPartition">Default partition.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task Create<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Read an Azure Cosmos DB entity with a document payload of type <c>T</c>.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document payload.</typeparam>
        /// <param name="id">Id of entity.</param>
        /// <param name="defaultPartition">Default partition.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Instance of object of type <c>T</c>.</returns>
        Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Replace/upsert an Azure Cosmos DB entity with a document payload of type <c>T</c>.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document payload.</typeparam>
        /// <param name="id">Id of entity.</param>
        /// <param name="item">Object of type T to be stored as the payload of the Cosmos DB entity.</param>
        /// <param name="defaultPartition">Default partition.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task Replace<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Deletes an Azure Cosmos DB entity with a document payload of type <c>T</c>.
        ///     </para>
        /// </summary>
        /// <param name="id">Id of entity.</param>
        /// <param name="defaultPartition">Default partition.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task Delete(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Get list of Azure Cosmos DB entities which contains a document payload of type <c>T</c> in the partition..
        ///     </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultPartition">Default partition.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Instance of a list of object of type <c>T</c>.</returns>
        Task<IEnumerable<T>> List<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Get list of json strings representing the document of type <c>T</c>in the partition.
        ///     </para>
        /// </summary>
        /// <param name="defaultPartition"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetPartitionDocuments(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);
    }
}

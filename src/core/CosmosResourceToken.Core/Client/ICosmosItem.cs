using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    /// <summary>
    ///     <para>
    ///         Interface for the Cosmos DB entity including a document payload for representing the object of type <c>T</c>.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">Type of document stored in Cosmos DB.</typeparam>
    public interface ICosmosItem<T> 
    {
        /// <summary>
        ///     <para>
        ///         Document of the type <c>T</c>.
        ///     </para>
        /// </summary>
        T Document { get; set; }

        /// <summary>
        ///     <para>
        ///         Unique Cosmos Entity id.
        ///     </para>
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     <para>
        ///         Converts <c>this</c> to a stream that can be send to Azure Cosmos DB.
        ///     </para>
        /// </summary>
        /// <param name="partitionKeyHeader">Partition key header.</param>
        /// <param name="partitionKey">Partition key.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns></returns>
        Task<Stream> ToStream(string partitionKeyHeader, string partitionKey, CancellationToken ct = default);

        /// <summary>
        ///     <para>
        ///         Converts a stream to an item of type <c>T</c>.
        ///         Item is identical to document of <c>T</c>.
        ///     </para>
        /// </summary>
        /// <param name="stream">Stream to be converted.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns></returns>
        Task<T> GetItemFromStream(Stream stream, CancellationToken ct = default);

        /// <summary>
        ///     <para>
        ///         Converts a stream to a list of items of type <c>T</c>.
        ///     </para>
        /// </summary>
        /// <param name="stream">Stream to be converted.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetItemsFromStream(Stream stream, CancellationToken ct = default);

        /// <summary>
        ///     <para>
        ///         Converts a stream to a list of json strings.
        ///     </para>
        /// </summary>
        /// <param name="stream">Stream to be converted.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetJsonStringsFromStream(Stream stream, CancellationToken ct = default);
    }
}

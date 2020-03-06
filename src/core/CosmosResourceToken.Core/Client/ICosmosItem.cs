using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    public interface ICosmosItem<T> 
    {
        T Document { get; set; }

        string Id { get; set; }

        Task<Stream> ToStream(string partitionKeyHeader, string partitionKey, CancellationToken ct = default);

        Task<T> GetItemFromStream(Stream stream, CancellationToken ct = default);

        Task<IEnumerable<T>> GetItemsFromStream(Stream stream, CancellationToken ct = default);

        Task<IEnumerable<string>> GetJsonStringsFromStream(Stream stream, CancellationToken ct = default);
    }
}

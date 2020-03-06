using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    [Preserve(AllMembers = true)]
    public interface ICosmosTokenClient
    {
        Task Create<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task Replace<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task Delete(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> List<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetPartitionDocuments(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);
    }
}

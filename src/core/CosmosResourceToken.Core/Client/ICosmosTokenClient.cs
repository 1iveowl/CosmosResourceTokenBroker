using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Client
{
    [Preserve(AllMembers = true)]
    public interface ICosmosTokenClient
    {
        Task Create<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task Replace<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task Delete<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default);
    }
}

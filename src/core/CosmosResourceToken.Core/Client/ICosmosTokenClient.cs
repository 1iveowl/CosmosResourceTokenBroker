using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Client
{
    [Preserve(AllMembers = true)]
    public interface ICosmosTokenClient
    {
        Task Create<T>(T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken);

        Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken);

        Task Replace<T>(T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken);

        Task Delete<T>(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken);

        Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken);
    }
}

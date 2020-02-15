using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    public interface ICosmosClient
    {
        Task Create<T>(string id, T obj, DefaultPartitionKind defaultPartition);

        Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition);

        Task Replace<T>(string id, T obj, DefaultPartitionKind defaultPartition);

        Task Delete<T>(string id, DefaultPartitionKind defaultPartition);

        Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition);
    }
}

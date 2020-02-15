using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Client
{
    public interface IResourceTokenBrokerClient
    {
        Task Create<T>(string id, T obj, DefaultPertitionsKind defaultPertition);

        Task<T> Read<T>(string id, DefaultPertitionsKind defaultPertition);

        Task Replace<T>(string id, T obj, DefaultPertitionsKind defaultPertition);

        Task Delete<T>(string id, DefaultPertitionsKind defaultPertition);

        Task<IEnumerable<T>> GetList<T>(DefaultPertitionsKind defaultPertition);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core
{
    public interface IResourceTokenBrokerService : IAsyncDisposable
    {
        Task<IPermissionToken> Get(string token, string userId, CancellationToken cancellationToken = default);
    }
}

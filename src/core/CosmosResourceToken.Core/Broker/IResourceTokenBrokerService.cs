using System;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Broker
{
    public interface IResourceTokenBrokerService : IAsyncDisposable
    {
        Task<IPermissionToken> Get(string userId, PermissionModeKind permissionMode, CancellationToken cancellationToken = default);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Broker
{
    [Preserve(AllMembers = true)]
    public interface IResourceTokenBrokerService : IAsyncDisposable
    {
        Task<IResourcePermissionResponse> Get(string userId, IEnumerable<IPermissionScope> permissionscopes, CancellationToken cancellationToken = default);
    }
}

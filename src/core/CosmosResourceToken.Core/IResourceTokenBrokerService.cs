using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CosmosResourceToken.Core
{
    public interface IResourceTokenBrokerService
    {

        Task<IPermissionToken> Get(string token, CancellationToken cancellationToken = default);

        Task<IPermissionToken> Get(string token, string userId, CancellationToken cancellationToken = default);
    }
}

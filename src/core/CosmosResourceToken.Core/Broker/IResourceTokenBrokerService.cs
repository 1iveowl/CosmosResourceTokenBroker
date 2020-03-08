using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceToken.Core.Broker
{
    /// <summary>
    ///     <para>
    ///         An interface for the Resource Token Broker Service handing out Resource Token to an authenticated user.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IResourceTokenBrokerService : IAsyncDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">The unique user id</param>
        /// <param name="permissionScopes">Permission scopes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IResourcePermissionResponse> Get(
            string userId, 
            IEnumerable<IPermissionScope> permissionScopes, 
            CancellationToken cancellationToken = default);
    }
}

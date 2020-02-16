using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    [Preserve(AllMembers = true)]
    public interface IB2CAuthService
    {
        Task<IUserContext> SignIn(CancellationToken cancellationToken = default);

        Task SignOut(CancellationToken cancellationToken = default);

        Task<IUserContext> AcquireUserContextForSpecificScope(string scope, CancellationToken cancellationToken = default);
    }
}

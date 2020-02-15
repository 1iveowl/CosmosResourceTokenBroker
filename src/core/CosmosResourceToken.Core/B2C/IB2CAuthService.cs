using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.B2C
{
    public interface IB2CAuthService
    {
        Task<IUserContext> SignIn(CancellationToken cancellationToken = default);

        Task SignOut(CancellationToken cancellationToken = default);
    }
}

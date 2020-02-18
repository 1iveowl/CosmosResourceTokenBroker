using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2CAuthClient.Abstract
{
    [Preserve(AllMembers = true)]
    public interface IB2CAuthService
    {
        IUserContext CurrentUserContext { get; }

        Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default);

        Task SignOut(CancellationToken cancellationToken = default);
    }
}

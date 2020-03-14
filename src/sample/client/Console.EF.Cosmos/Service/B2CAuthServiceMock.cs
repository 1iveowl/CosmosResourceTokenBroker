using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;

namespace Console.EF.Cosmos.Service
{
    public class B2CAuthServiceMock : AuthMock, IB2CAuthService
    {
        public IUserContext CurrentUserContext => GetUserContext();
        public bool IsInterativeSignInInProgress => false;

        public async Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(GetUserContext());
        }

        public async Task SignOut(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}

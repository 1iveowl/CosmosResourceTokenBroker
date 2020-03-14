using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;

namespace ResourceTokenBrokerTest.Service
{
    public class TestB2CAuthService : IB2CAuthService
    {
        private readonly IUserContext _userContext;

        public IUserContext CurrentUserContext => _userContext;
        public bool IsInterativeSignInInProgress { get; }

        public TestB2CAuthService(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_userContext);
        }

        public async Task SignOut(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}

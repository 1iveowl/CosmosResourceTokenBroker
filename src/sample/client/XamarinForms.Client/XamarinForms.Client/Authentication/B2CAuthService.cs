using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using Microsoft.Identity.Client;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinForms.Client.Authentication.Interface;
using XamarinForms.Client.Authentication.Model;

namespace XamarinForms.Client.Authentication
{
    public class B2CAuthService : IB2CAuthService
    {
        private readonly IPublicClientApplication _pca;
        private readonly IEnumerable<string> _scopes;
        private readonly string _signUpSignInFlowName;

        internal B2CAuthService(
            string b2chostName, 
            string tenantId, 
            string clientId, 
            string signUpSignInFlowName, 
            IEnumerable<string> scopes, 
            string iOsChainGroup, 
            DevicePlatform devicePlatform)
        {
            _scopes = scopes;
            _signUpSignInFlowName = signUpSignInFlowName;
            
            var builder = PublicClientApplicationBuilder.Create(clientId)
                .WithB2CAuthority($"https://{b2chostName}" +
                                  $"/tfp/{tenantId}" +
                                  $"/{signUpSignInFlowName}")
                .WithRedirectUri($"msal{clientId}://auth");

            if (devicePlatform == DevicePlatform.Android)
            {
                // Android utilizes: https://github.com/jamesmontemagno/CurrentActivityPlugin
                builder.WithParentActivityOrWindow(() =>
                    DependencyService.Get<IParentWindowLocatorService>().GetCurrentParentWindow());
            }

            if (devicePlatform == DevicePlatform.iOS 
                || devicePlatform == DevicePlatform.tvOS 
                || devicePlatform == DevicePlatform.watchOS)
            {
                builder.WithIosKeychainSecurityGroup(iOsChainGroup);
            }

            _pca = builder.Build();
        }

        public async Task<IUserContext> SignIn(CancellationToken cancellationToken = default)
        {
            return await AcquireToken(_scopes, cancellationToken);
        }
        
        public async Task SignOut(CancellationToken cancellationToken = default)
        {
            var account = await GetAccount();

            await _pca.RemoveAsync(account);
        }


        //See this: https://medium.com/@smartdeveloper/azure-functions-rest-api-security-with-msal-and-azure-ad-c9cd75d3316e
        public async Task<IUserContext> AcquireUserContextForSpecificScope(string scope, CancellationToken cancellationToken = default)
        {
            return await AcquireToken(new List<string> { scope }, cancellationToken);
        }

        private async Task<IUserContext> AcquireToken(IEnumerable<string> scopes, CancellationToken ct)
        {
            //TODO Read this: https://medium.com/@smartdeveloper/azure-functions-rest-api-security-with-msal-and-azure-ad-c9cd75d3316e
            var account = await GetAccount();

            try
            {
                var authResult = await _pca.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(ct);

                return new UserContext(authResult);
            }
            catch (MsalUiRequiredException)
            {
                return await SignInInteractively(account, ct);
            }
        }

        private async Task<IUserContext> SignInInteractively(IAccount account, CancellationToken ct)
        {
            var authResult = await _pca.AcquireTokenInteractive(_scopes)
                .WithAccount(account)
                .ExecuteAsync(ct);

            return new UserContext(authResult);
        }

        private async Task<IAccount> GetAccount()
        {
            var accounts = await _pca.GetAccountsAsync();

            return accounts.FirstOrDefault(a =>
                a.HomeAccountId?.ObjectId?.Split('.')?[0]?.EndsWith(_signUpSignInFlowName.ToLowerInvariant()) ?? false);
        }
    }
}

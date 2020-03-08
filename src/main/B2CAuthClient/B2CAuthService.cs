﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using Microsoft.Identity.Client;

namespace B2CAuthClient
{
    /// <summary>
    ///     <para>
    ///         An implementation of a Azure AD B2S Authentication Service.
    ///     </para>
    ///     <para>
    ///         Implements <c>IB2CAuthService</c>
    ///     </para>
    /// </summary>
    public class B2CAuthService : IB2CAuthService
    {
        private readonly IPublicClientApplication _pca;
        private readonly IEnumerable<string> _defaultScopes;
        private readonly string _signUpSignInFlowName;

        /// <summary>
        ///     <para>
        ///         Get the current user context of a log-in user.
        ///     </para>
        ///     <para>
        ///         Null if no user has signed in.
        ///     </para>
        /// </summary>
        public IUserContext CurrentUserContext { get; private set; }


        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="B2CAuthService"></see>
        ///     </para>
        /// </summary>
        /// <param name="b2cHostName">Azure AD B2C Host name</param>
        /// <param name="tenantId">Azure AD B2C Tenant Id</param>
        /// <param name="clientId">Azure AD B2C Client ID - aka. Application Id</param>
        /// <param name="signUpSignInFlowName">Azure AD B2C Sign In Flow name </param>
        /// <param name="defaultScopes">Azure Ad B2C Scopes</param>
        /// <param name="iOsChainGroup">iOS Chain Group</param>
        /// <param name="isAndroidDeviceFunc">Func that returns <c>true</c> at run-time if the device is an Android device.</param>
        /// <param name="isAppleDeviceFunc">Func that returns <c>true</c> at run-time if the device is an iOS device.</param>
        /// <param name="getCurrentParentWindowsForAndroidFunc">Func that returns the Parent Window for Android device.</param>
        public B2CAuthService(
            string b2cHostName, 
            string tenantId, 
            string clientId, 
            string signUpSignInFlowName, 
            IEnumerable<string> defaultScopes, 
            string iOsChainGroup, 
            Func<bool> isAndroidDeviceFunc,
            Func<bool> isAppleDeviceFunc,
            Func<object> getCurrentParentWindowsForAndroidFunc)
        {
            _defaultScopes = defaultScopes;
            _signUpSignInFlowName = signUpSignInFlowName;
            
            var builder = PublicClientApplicationBuilder.Create(clientId)
                .WithB2CAuthority($"https://{b2cHostName}" +
                                  $"/tfp/{tenantId}" +
                                  $"/{signUpSignInFlowName}")
                .WithRedirectUri($"msal{clientId}://auth");

            if (isAndroidDeviceFunc())
            {
                builder.WithParentActivityOrWindow(getCurrentParentWindowsForAndroidFunc);
            }

            if (isAppleDeviceFunc())
            {
                builder.WithIosKeychainSecurityGroup(iOsChainGroup);
            }

            _pca = builder.Build();
        }

        public async Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default)
        {
            IUserContext userContext;

            if (scopes is null)
            {
                userContext = await AcquireToken(silentlyOnly, _defaultScopes, cancellationToken);
            }
            else
            {
                userContext = await AcquireToken(silentlyOnly, scopes, cancellationToken);
            }

            CurrentUserContext = userContext;

            return userContext;
        }
        
        public async Task SignOut(CancellationToken cancellationToken = default)
        {
            var account = await GetAccount();

            if (account is null)
            {
                return;
            }

            await _pca.RemoveAsync(account);

            CurrentUserContext = new UserContext();

        }

        private async Task<IUserContext> AcquireToken(bool silentlyOnly, IEnumerable<string> scopes, CancellationToken ct)
        {
            var account = await GetAccount();

            try
            {
                var authResult = await _pca.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(ct);

                return new UserContext(authResult);
            }
            catch (MsalUiRequiredException ex)
            {
                if (silentlyOnly)
                {
                    throw ex;
                }

                return await SignInInteractively(account, ct);
            }
        }

        private async Task<IUserContext> SignInInteractively(IAccount account, CancellationToken ct)
        {
            try
            {
                AuthenticationResult authResult;

                if (account is null)
                {
                    authResult = await _pca.AcquireTokenInteractive(_defaultScopes)
                        .ExecuteAsync(ct);
                }
                else
                {
                    authResult = await _pca.AcquireTokenInteractive(_defaultScopes)
                        .WithAccount(account)
                        .ExecuteAsync(ct);
                }
            
                return new UserContext(authResult);
            }
            catch (Exception ex)
            {
                throw new B2CAuthClientException($"Unable to acquire token interactive. Unhandled exception: {ex}");
            }
        }

        private async Task<IAccount> GetAccount()
        {
            var accounts = await _pca.GetAccountsAsync();

            return accounts.FirstOrDefault(a =>
                a.HomeAccountId?.ObjectId?.Split('.')?[0]?.EndsWith(_signUpSignInFlowName.ToLowerInvariant()) ?? false);
        }
    }
}

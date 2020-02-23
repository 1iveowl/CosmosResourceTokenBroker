using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceTokenClient
{
    [Preserve(AllMembers = true)]
    internal class CosmosTokenClientHandler : IAsyncDisposable
    {
        private readonly IB2CAuthService _authService;
        private readonly ResourceTokenBrokerClientService _brokerClient;
        private readonly ICacheSingleObjectByKey _resourceTokenCache;

        internal CosmosTokenClientHandler(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey resourceTokenCache = null)
        {
            _authService = authService ?? throw new NoNullAllowedException("B2C Authentication Service construction parameter cannot be null");

            _brokerClient = new ResourceTokenBrokerClientService(resourceTokenBrokerUrl);
            _resourceTokenCache = resourceTokenCache;
        }

        internal async Task Execute(Func<IResourcePermissionResponse, Task> cosmosfunc, PermissionModeKind permissionMode, CancellationToken ct)
        {
            await Execute(async resourcePermissionResponse =>
            {
                await cosmosfunc(resourcePermissionResponse);
                return true;
            }, permissionMode, ct);
        }

        internal async Task<T> Execute<T>(Func<IResourcePermissionResponse, Task<T>> cosmosfunc, PermissionModeKind permissionMode, CancellationToken ct)
        {
            await ValidateLoginState();
            
            var resourcePermissionResponse = await AcquireResourceToken(_authService.CurrentUserContext, ct);

            var resourceToken = resourcePermissionResponse?.ResourcePermissions?
                .FirstOrDefault(p => p?.PermissionMode == permissionMode)?.ResourceToken;

            if (string.IsNullOrEmpty(resourceToken))
            {
                throw new CosmosClientException($"No Resource Token acquired for permission: {permissionMode.ToString()}");
            }

            var endpointUrl = resourcePermissionResponse?.EndpointUrl;

            if (string.IsNullOrEmpty(endpointUrl) || !Uri.TryCreate(endpointUrl, UriKind.Absolute, out _))
            {
                throw new CosmosClientException($"No or invalid endpoint received from broker: {endpointUrl}");
            }

            var databaseId = resourcePermissionResponse?.DatabaseId;

            if (string.IsNullOrEmpty(databaseId))
            {
                throw new CosmosClientException($"No or invalid database Id received from broker: {databaseId}");
            }

            var collectionId = resourcePermissionResponse?.CollectionId;

            if (string.IsNullOrEmpty(collectionId))
            {
                throw new CosmosClientException($"No or invalid database Id received from broker: {collectionId}");
            }
            
            return await cosmosfunc(resourcePermissionResponse);
        }

        private async Task<IResourcePermissionResponse> AcquireResourceToken(IUserContext userContext, CancellationToken ct)
        {
            
            if (_resourceTokenCache is null)
            {
                return await _brokerClient.GetResourceToken(userContext.AccessToken, ct);
            }

            return await GetResourceTokenThroughCache(userContext, ct);
        }

        private async Task<IResourcePermissionResponse> GetResourceTokenThroughCache(IUserContext userContext, CancellationToken ct)
        {
            return await _resourceTokenCache.TryGetFromCache(
                userContext.UserIdentifier,
                RenewObjectFunc,
                IsCachedObjectValidFunc);

            // local function fetching a new Resource Permission Response from the Resource Token Broker
            // User the first time and whenever an existing Resource Permission Response object in the cache has expired.
           Task<IResourcePermissionResponse> RenewObjectFunc() => _brokerClient.GetResourceToken(userContext.AccessToken, ct);

            // local function evaluating the validity of the object stored in the cache.
            static Task<bool> IsCachedObjectValidFunc(IResourcePermissionResponse cachedPermissionObj)
            {
                if (cachedPermissionObj is null)
                {
                    return Task.FromResult(false);
                }

                // Get the Permission that is closed to expire.
                var expires = cachedPermissionObj.ResourcePermissions?.OrderBy(resourcePermission => resourcePermission.ExpiresUtc)
                    .FirstOrDefault()
                    ?.ExpiresUtc ?? default;

                // Set expiration permission five minutes before actual expiration to be on safe side. 
                return Task.FromResult(DateTime.UtcNow <= expires - TimeSpan.FromMinutes(5));
            }
        }

        private async Task ValidateLoginState()
        {
            if (_authService.CurrentUserContext is null || !_authService.CurrentUserContext.IsLoggedOn || _authService.CurrentUserContext.HasAccessTokenExpired)
            {
                // If not logged on then try silently to acquire user context.
                try
                {
                    var userContext = await _authService.SignIn(silentlyOnly: true);

                    if (!userContext.IsLoggedOn)
                    {
                        throw new CosmosClientAuthenticationException("User must be logged in to Azure B2C with a valid and non-expired access token. " +
                                                                      "Try to login in interactively before continuing.");
                    }
                }
                catch (Exception ex)
                {
                    throw new CosmosClientAuthenticationException("User must be logged in to Azure B2C with a valid and non-expired access token. " +
                                                                  "Try to login in interactively before continuing.", ex);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _brokerClient.DisposeAsync();
        }
    }
}

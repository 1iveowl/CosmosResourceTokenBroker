using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceTokenClient
{
    public abstract class CosmosTokenClientHandler
    {
        private readonly IB2CAuthService _authService;
        private readonly ResourceTokenBrokerClientService _brokerClient;
        private readonly ICacheSingleObjectByKey<IResourcePermissionResponse> _resourceTokenCache;

        internal CosmosTokenClientHandler(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey<IResourcePermissionResponse> resourceTokenCache = null)
        {
            _authService = authService ?? throw new NoNullAllowedException("B2C Authentication Service construction parameter cannot be null");

            _brokerClient = new ResourceTokenBrokerClientService(resourceTokenBrokerUrl);
            _resourceTokenCache = resourceTokenCache;
        }

        protected async Task<T> Execute<T>(Func<IResourcePermissionResponse, Task<T>> cosmosfunc, PermissionModeKind permissionMode)
        {
            await ValidateLoginState();
            
            var resourcePermissionResponse = await GetResourceToken(_authService.CurrentUserContext);

            var resourceToken =
                resourcePermissionResponse?.ResourcePermissions?.FirstOrDefault(p => p?.PermissionMode == permissionMode)?.ResourceToken;

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

        private async Task<IResourcePermissionResponse> GetResourceToken(IUserContext userContext)
        {
            if (_resourceTokenCache is null)
            {
                return await _brokerClient.GetResourceToken(userContext.AccessToken);
            }

            return await GetResourceTokenThroughCache(userContext);
        }

        private async Task<IResourcePermissionResponse> GetResourceTokenThroughCache(IUserContext userContext)
        {
            var (cacheState, resourcePermissions) = await _resourceTokenCache.TryGetFromCache(userContext.UserIdentifier);

            if (cacheState != CacheObjectStateKind.Ok)
            {
                resourcePermissions = await _brokerClient.GetResourceToken(userContext.AccessToken);

                var expires = resourcePermissions?.ResourcePermissions?
                    .OrderBy(resourcePermission => resourcePermission.ExpiresUtc)
                    .FirstOrDefault()?.ExpiresUtc ?? default;

                var cacheKey = resourcePermissions?.UserId;

                if (!string.IsNullOrEmpty(cacheKey) && !(expires == default))
                {
                    await _resourceTokenCache.CacheObject(resourcePermissions, cacheKey, expires);
                }
            }

            return resourcePermissions;
        }

        private async Task ValidateLoginState()
        {
            if (_authService.CurrentUserContext is null || !_authService.CurrentUserContext.IsLoggedOn || !_authService.CurrentUserContext.HasAccessTokenExpired)
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
    }
}

//using System;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using CosmosResourceToken.Core;
//using Microsoft.Azure.Documents;
//using Microsoft.Azure.Documents.Client;
//using Microsoft.Extensions.Logging;

//namespace CosmosResourceTokenBroker
//{
//    public class ResourceTokenBrokerService : IResourceTokenBrokerService
//    {
//        private readonly Uri _hostUri;
//        private readonly DocumentClient _documentClient;
//        private readonly string _databaseId;
//        private readonly string _collectionId;
        
//        private readonly ILogger _logger;

//        private readonly DateTime _beginningOfTime;

//        private int CreateExpires() => Convert.ToInt32(DateTime.UtcNow.Subtract(_beginningOfTime).Add(TimeSpan.FromHours(1)).TotalSeconds);

//        private static string CreatePermissionId(string userId) => $"user-{userId}-permission";

//        public ResourceTokenBrokerService(
//            string hostUrl,
//            string key,
//            string databaseId,
//            string collectionId,
//            ILogger logger = default)
//        {
//            _hostUri = new Uri($"{hostUrl}/.auth/me");
//            _databaseId = databaseId;
//            _collectionId = collectionId;
//            _logger = logger;

//            _documentClient = new DocumentClient(new Uri(hostUrl), key);

//            _beginningOfTime = new DateTime(2017, 1, 1);
//        }

//        public async Task<IPermissionToken> Get(string userId, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await GetPermission(userId, cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                var error = $"Unable to acquire user id. Exception thrown: {ex}";
//                _logger?.Log(LogLevel.Error, ex, "");

//                throw new ResourceTokenBrokerServiceException(error, ex);
//            }
//        }

//        private async Task<IPermissionToken> GetPermission(string userId, CancellationToken ct)
//        {
//            var permissionToken = await GetCachedUserPermission(userId, ct) ?? await CreateNewPermission(userId, ct);

//            return permissionToken;
//        }

//        private async Task<IPermissionToken> CreateNewPermission(string userId, CancellationToken ct)
//        {
//            if (await GetNewPermission(userId) is PermissionToken permissionToken)
//            {
//                await CachePermission(permissionToken, ct);
//            }
//            else
//            {
//                throw new ResourceTokenBrokerServiceException("Unable to create new Permission");
//            }

//            return permissionToken;

//            async Task CachePermission(PermissionToken pt, CancellationToken ct)
//            {
//                pt.Id = CreatePermissionId(pt.UserId);
//                await _documentClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), pt, cancellationToken: ct);
//            }
//        }

//        private async Task<IPermissionToken> GetCachedUserPermission(string userId, CancellationToken ct)
//        {
//            try
//            {
//                var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, $"user-{userId}");

//                var requestOption = new RequestOptions {PartitionKey = new PartitionKey(userId)};

//                var resourceResponse = await _documentClient.ReadDocumentAsync(documentUri, requestOption, ct);

//                if (!(resourceResponse?.Resource is null))
//                {
//                    var expires = resourceResponse.Resource.GetPropertyValue<int>("expires");

//                    if (expires > FiveMinutesAgo())
//                    {
//                        return new PermissionToken
//                        {
//                            Token = resourceResponse.Resource.GetPropertyValue<string>("token"),
//                            ExpiresUtc = expires,
//                            UserId = userId
//                        };
//                    }
//                }
//            }
//            catch (DocumentClientException ex)
//            {
//                if (ex.StatusCode == HttpStatusCode.NotFound)
//                {
//                    return null;
//                }
//                else
//                {
//                    throw new ResourceTokenBrokerServiceException($"Unable to read cached user permissions for user {userId}", ex);
//                }
//            }

//            return null;

//            int FiveMinutesAgo() => Convert.ToInt32(DateTime.UtcNow.AddMinutes(-5).Subtract(_beginningOfTime).TotalSeconds);
//        }

//        private async Task<IPermissionToken> GetNewPermission(string userId)
//        {
//            try
//            {
//                var permission = await _documentClient.ReadPermissionAsync(UriFactory.CreatePermissionUri(
//                    _databaseId, 
//                    userId, 
//                    CreatePermissionId(userId)));

//                if (!(permission?.Resource is null))
//                {
//                    return new PermissionToken
//                    {
//                        Token = permission.Resource.Token,
//                        ExpiresUtc = CreateExpires(),
//                        UserId = userId
//                    };
//                }
//            }
//            catch (DocumentClientException ex)
//            {
//                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
//                {
//                    return await CreateNewPermission(userId);
//                }

//                throw new ResourceTokenBrokerServiceException($"Unable to read permissions for user: {userId}", ex);
//            }

//            return default;
//        }

//        private async Task<IPermissionToken> CreateNewPermission(string userId)
//        {
//            var collection = await _documentClient
//                .ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));

//            if (!(collection?.Resource is null))
//            {
//                var newPermission = new Permission
//                {
//                    PermissionMode = PermissionMode.All,
//                    ResourceLink = collection.Resource.SelfLink,
//                    ResourcePartitionKey = new PartitionKey(userId),
//                    Id = CreatePermissionId(userId) //needs to be unique for a given user
//                };

//                await LookupUserAndCreateIfNotExist(userId);

//                var permission =
//                    await _documentClient.CreatePermissionAsync(UriFactory.CreateUserUri(_databaseId, userId),
//                        newPermission);

//                if (!(permission?.Resource is null))
//                {
//                    return new PermissionToken
//                    {
//                        Token = permission.Resource.Token,
//                        ExpiresUtc = CreateExpires(),
//                        UserId = userId
//                    };
//                }
//            }

//            return default;
//        }

//        private async Task LookupUserAndCreateIfNotExist(string userId)
//        {
//            try
//            {
//                var t = await _documentClient.ReadUserAsync(UriFactory.CreateUserUri(_databaseId, userId));
//            }
//            catch (DocumentClientException ex)
//            {
//                if (ex.StatusCode == HttpStatusCode.NotFound)
//                {
//                    await _documentClient.CreateUserAsync(
//                        UriFactory.CreateDatabaseUri(_databaseId),
//                        new User { Id = userId });
//                }

//                throw new ResourceTokenBrokerServiceException($"Unable to read user {userId}", ex);
//            }
//        }

//        public ValueTask DisposeAsync()
//        {
//            _documentClient?.Dispose();

//            return new ValueTask();
//        }
//    }
//}

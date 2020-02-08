using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosResourceToken.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosResourceTokenBroker
{
    public class ResourceTokenBrokerService : IResourceTokenBrokerService
    {
        private readonly Uri _hostUri;
        private readonly DocumentClient _documentClient;
        private readonly string _databaseId;
        private readonly string _collectionId;
        
        private readonly ILogger _logger;

        public ResourceTokenBrokerService(
            string hostUrl,
            DocumentClient documentClient,
            string databaseId,
            string collectionId,
            ILogger logger = default)
        {
            _hostUri = new Uri($"{hostUrl}/.auth/me");
            _documentClient = documentClient;
            _databaseId = databaseId;
            _collectionId = collectionId;
            _logger = logger;
            

        }

        public async Task<IActionResult> Get(string token)
        {
            try
            {
                var (userId, response) = await GetUserId(token);

                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
                else
                {
                    var error =  $"Unable to acquire user id. Status code: {response.StatusCode}, Reason {response.ReasonPhrase}";
                    _logger.Log(LogLevel.Error, error);

                    return new BadRequestObjectResult(error);
                }
            }
            catch (Exception ex)
            {
                var error = $"Unable to acquire user id. Exception thrown: {ex}";
                _logger.Log(LogLevel.Error, ex, "");

                return new BadRequestObjectResult(error);
            }
        }

        public Task<IActionResult> Get(string token, string userId, string userPermissionId)
        {
            
        }


        private async Task<IPermissionToken> GetNewPermission(UserResponse userResponse, string userPermissionId)
        {
            Permission permission = null;

            try
            {

                permission = userResponse.User.GetPermission(userPermissionId);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private async Task<PermissionToken> GetCachedUserPermission(string userId)
        {
            var permissionDocument = await _db.ReadAsync();
        }

        private async Task LookupUserAndCreateIfNotExist(Uri userLinkUri, string userId)
        {
            try
            {
                await _documentClient.ReadUserAsync(userLinkUri);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await _documentClient.CreateUserAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        new User {Id = userId});
                }
            }
        }

        private async Task<(string userId, HttpResponseMessage response)> GetUserId(string token)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("x-zumo-auth", token);

            var response = await httpClient.GetAsync(_hostUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonSerializer = new JsonSerializer();

                await using var contentStream = await response.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var obj = jsonSerializer.Deserialize<JArray>(jsonReader);

                    var userId = obj?
                        .Children()
                        .FirstOrDefault()?
                        .Children<JProperty>()
                        .FirstOrDefault(x => x?.Name?.ToLower() == "user_id")?.Value?.ToString();

                    return (userId, response);
                }
            }

            return (null, response);
        }
    }
}

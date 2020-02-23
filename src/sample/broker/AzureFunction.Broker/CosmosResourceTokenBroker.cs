using System;
using System.Threading.Tasks;
using CosmosResourceTokenBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Newtonsoft.Json;
using static CosmosResourceToken.Core.Model.Constants;

namespace AzureFunction.Broker
{
    public class CosmosResourceTokenBroker
    {
        private readonly string _cosmosHostUrl;
        private readonly string _cosmosKey;
        private readonly string _cosmosDatabaseId;
        private readonly string _cosmosCollectionId;
        
        public CosmosResourceTokenBroker()
        {
            // loading configuration variables.
            _cosmosHostUrl = Environment.GetEnvironmentVariable("CosmosUrl");
            _cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
            _cosmosDatabaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            _cosmosCollectionId = Environment.GetEnvironmentVariable("CosmosCollectionId");
        }

        [FunctionName("CosmosResourceTokenBroker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "broker")] HttpRequest req,
            ILogger log)
        {

            // Extracting the Access Token from the http request from the client
            string accessToken = null;

            try
            {
                accessToken = req?.Headers?["Authorization"].ToString()?.Replace("Bearer ", string.Empty);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return LogErrorAndReturnBadObjectResult("Access token is missing", log);
                }
            }
            catch (Exception ex)
            {
                return LogErrorAndReturnBadObjectResult("Unable to read Authorization header", log, ex);
            }


            // Decrypting and reading the Access Token
            JwtSecurityToken token = null;

            try
            {
                var handler = new JwtSecurityTokenHandler();

                token = handler.ReadJwtToken(accessToken);
                
                log.Log(LogLevel.Information, $"Jwt: {token}");

                if (!handler.CanValidateToken)
                {
                    return LogErrorAndReturnBadObjectResult($"Unable to validate token: {accessToken}", log);
                }
            }
            catch (Exception ex)
            {
                return LogErrorAndReturnBadObjectResult($"Unable to read JWT token: {accessToken}", log, ex);
            }

#if DEBUG
            var expires = token.ValidTo;

            var ignoreExpires = req?.Headers?["IgnoreExpires"].ToString().ToLower() == "true";

            if (DateTime.UtcNow > expires && !ignoreExpires)
            {
                return LogErrorAndReturnBadObjectResult("The access token have expired. " +
                                                        "Note this error is for debug mode only, for use when testing. " +
                                                        "In production access token validity is handled by Azure Functions configuration.", log);
            }
#endif

            // Getting the user object id from the Access Token
            var userObjectId = token?.Subject;

            if (string.IsNullOrEmpty(userObjectId))
            {
                return LogErrorAndReturnBadObjectResult("No subject defined in access token", log);
            }

            // Getting the permission scope from the Access Token to determine the permission mode.
            var accessTokenScopes = token?.Claims.FirstOrDefault(c => c.Type.ToLowerInvariant() == "scp")?.Value.Split(' ');

            if (!accessTokenScopes?.Any() ?? false)
            {
                return LogErrorAndReturnBadObjectResult("No scopes defined", log);
            }


            // Extracting the known scopes only.
            var permissionScopes = accessTokenScopes?.Select(scope => KnownPermissionScopes?.FirstOrDefault(ks => ks?.Scope == scope));
            
            if (!permissionScopes?.Any() ?? false)
            {
                return LogErrorAndReturnBadObjectResult($"No known scopes: " +
                                                        $"{string.Join(", ", accessTokenScopes)}. " +
                                                        $"Known scopes are: " +
                                                        $"{string.Join(", ", KnownPermissionScopes.Select(ks => ks.Scope))}", log);
            }

            try
            {

                await using var brokerService = new ResourceTokenBrokerService(_cosmosHostUrl, _cosmosKey, _cosmosDatabaseId, _cosmosCollectionId);

                // Getting the Resource Permission Tokens
                var permissionToken = await brokerService.Get(userObjectId, permissionScopes);

                return (IActionResult) new JsonResult(permissionToken, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            }
            catch (Exception ex)
            {
                return LogErrorAndReturnBadObjectResult($"Unable to acquire resource token from Cosmos DB " +
                                                        $"for user with id: {userObjectId} " +
                                                        $"for permission scopes: {string.Join(", ", permissionScopes)}.", log, ex);
            }
        }

        private static BadRequestObjectResult LogErrorAndReturnBadObjectResult(string error, ILogger log, Exception ex = default)
        {
            log.Log(LogLevel.Error, ex is null ? $"{error}" : $"{error}. Unhandled exception: {ex}");
            return new BadRequestObjectResult(ex is null ? $"{error}" :$"{error}. Unhandled exception: {ex}");
        }
    }
}

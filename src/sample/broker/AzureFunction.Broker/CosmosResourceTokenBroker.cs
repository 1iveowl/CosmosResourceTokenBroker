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
using CosmosResourceToken.Core;
using CosmosResourceToken.Core.Model;

namespace AzureFunction.Broker
{
    public class CosmosResourceTokenBroker
    {
        private readonly string _cosmosHostUrl;
        private readonly string _cosmosKey;
        private readonly string _cosmosDatabaseId;
        private readonly string _cosmosCollectionId;
        private readonly string _permissionModeReadScopeName;
        private readonly string _permissionModeAllScopeName;
        
        public CosmosResourceTokenBroker()
        {
            // loading configuration variables.
            _cosmosHostUrl = Environment.GetEnvironmentVariable("CosmosUrl");
            _cosmosKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            _cosmosDatabaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            _cosmosCollectionId = Environment.GetEnvironmentVariable("CosmosCollectionId");
            _permissionModeAllScopeName = Environment.GetEnvironmentVariable("PermissionModeAllScopeName");
            _permissionModeReadScopeName = Environment.GetEnvironmentVariable("PermissionModeReadScopeName");
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
                    return LogErrorAndCreateBadObjectResult("Access token is missing", log);
                }
            }
            catch (Exception ex)
            {
                return LogErrorAndCreateBadObjectResult("Unable to read Authorization header", log, ex);
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
                    return LogErrorAndCreateBadObjectResult($"Unable to validate token: {accessToken}", log);
                }
            }
            catch (Exception ex)
            {
                return LogErrorAndCreateBadObjectResult($"Unable to read JWT token: {accessToken}", log, ex);
            }

            // Getting the user object id from the Access Token
            var userObjectId = token?.Subject;
            
            if (string.IsNullOrEmpty(userObjectId))
            {
                return LogErrorAndCreateBadObjectResult("No subject defined in access token", log);
            }

            // Getting the permission scope from the Access Token to determine the permission mode.
            var permissionScopes = token?.Claims.FirstOrDefault(c => c.Type.ToLowerInvariant() == "scp")?.Value.Split(' ');
            
            PermissionModeKind permissionMode;

            if (permissionScope?.ToLower() == _permissionModeReadScopeName.ToLower())
            {
                permissionMode = PermissionModeKind.Read;
            }
            else if (permissionScope?.ToLower() == _permissionModeAllScopeName.ToLower())
            {
                permissionMode = PermissionModeKind.All;
            }
            else
            {
                return LogErrorAndCreateBadObjectResult("Unknown scope", log);
            }

            // Instantiating the Resource Token Broker Service
            await using var brokerService = new BrokerService(_cosmosHostUrl, _cosmosKey, _cosmosDatabaseId, _cosmosCollectionId);

            // Getting the Resource Permission Token
            var permissionToken = await brokerService.Get(userObjectId, permissionMode);

            return (IActionResult) new OkObjectResult(permissionToken);
        }

        private static BadRequestObjectResult LogErrorAndCreateBadObjectResult(string error, ILogger log, Exception ex = default)
        {
            log.Log(LogLevel.Error, ex is null ? $"{error}" : $"{error}. Unhandled exception: {ex}");
            return new BadRequestObjectResult(ex is null ? $"{error}" :$"{error}. Unhandled exception: {ex}");
        }
    }
}

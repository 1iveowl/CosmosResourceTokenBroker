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

namespace AzureFunction.Broker
{
    public class CosmosResourceTokenBroker
    {
        private readonly string _hostUrl;
        private readonly string _key;
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly string _permissionModeReadScopeName;
        private readonly string _permissionModeAllScopeName;
        
        public CosmosResourceTokenBroker()
        {
            _hostUrl = Environment.GetEnvironmentVariable("CosmosUrl");
            _key = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            _databaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            _collectionId = Environment.GetEnvironmentVariable("CosmosCollectionId");
            _permissionModeAllScopeName = Environment.GetEnvironmentVariable("PermissionModeAllScopeName");
            _permissionModeReadScopeName = Environment.GetEnvironmentVariable("PermissionModeReadScopeName");
        }

        [FunctionName("CosmosResourceTokenBroker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "broker")] HttpRequest req,
            ILogger log)
        {
            
            string accessToken = null;

            try
            {
                accessToken = req?.Headers?["Authorization"].ToString()?.Replace("Bearer ", string.Empty);

                log.Log(LogLevel.Information, $"Access token: {accessToken}");

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new BadRequestObjectResult("Access token is missing");
                }
            }
            catch (Exception ex)
            {
                var error = $"Auth header: {accessToken} - Exception: {ex}";
                log.Log(LogLevel.Error, error);
                return new BadRequestObjectResult(error);
            }

            JwtSecurityToken token = null;

            try
            {
                var handler = new JwtSecurityTokenHandler();

                token = handler.ReadJwtToken(accessToken);
                
                log.Log(LogLevel.Information, $"Jwt: {token}");

                if (!handler.CanValidateToken)
                {
                    var error = "Unable to validate token.";
                    log.Log(LogLevel.Error, error);
                    return new BadRequestObjectResult(error);
                }
            }
            catch (Exception ex)
            {
                var error = $"Jwt token: {token} - Exception: {ex}";
                log.Log(LogLevel.Error, error);
                return new BadRequestObjectResult(error);
            }

            var userObjectId = token?.Subject;

            if (string.IsNullOrEmpty(userObjectId))
            {
                var error = "No subject defined in access token";
                log.Log(LogLevel.Error, error);
                return new BadRequestObjectResult(error);
            }

            var permissionScope = token?.Claims.FirstOrDefault(c => c.Type.ToLowerInvariant() == "scp")?.Value;
            
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
                var error = "Unknown scope";
                log.Log(LogLevel.Error, error);
                return new BadRequestObjectResult(error);
            }

            await using var brokerService = new BrokerService(_hostUrl, _key, _databaseId, _collectionId);

            var permissionToken = await brokerService.Get(userObjectId, permissionMode);

            return (IActionResult) new OkObjectResult(permissionToken);
        }
    }
}

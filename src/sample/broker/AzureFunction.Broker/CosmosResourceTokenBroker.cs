using System;
using System.Threading.Tasks;
using CosmosResourceTokenBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel;
using System.IdentityModel.Tokens.Jwt;

namespace AzureFunction.Broker
{
    public class CosmosResourceTokenBroker
    {
        private readonly string _hostUrl;
        private readonly string _key;
        private readonly string _databaseId;
        private readonly string _collectionId;
        
        public CosmosResourceTokenBroker()
        {
            _hostUrl = Environment.GetEnvironmentVariable("CosmosUrl");
            _key = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            _databaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            _collectionId = Environment.GetEnvironmentVariable("CosmosCollection");
        }

        [FunctionName("CosmosResourceTokenBroker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "broker")] HttpRequest req,
            ILogger log)
        {
            log.Log(LogLevel.Trace, "Start");
            await using var brokerService = new BrokerService2(_hostUrl, _key, _databaseId, _collectionId);

            string accessToken = null;

            try
            {
                accessToken = req?.Headers?["Authorization"].ToString()?.Replace("Bearer ", string.Empty);

                log.Log(LogLevel.Information, $"Access token: {accessToken}");
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
            }
            catch (Exception ex)
            {
                var error = $"Jwt token: {token} - Exception: {ex}";
                log.Log(LogLevel.Error, error);
                return new BadRequestObjectResult($"Jwt token: {token} - Exception: {ex}");
            }

            var userObjectId = token?.Subject;

            var permissionToken = await brokerService.Get(userObjectId);

            return (IActionResult) new OkObjectResult($"User: {userObjectId}");

                //if (!string.IsNullOrEmpty(accessToken))
                //{

                //    if (req.GetQueryParameterDictionary().TryGetValue("userId", out var userId))
                //    {
                //        if (!string.IsNullOrEmpty(userId.Trim()))
                //        {
                //            var permission = await brokerService.Get(accessToken, userId);

                //            return (IActionResult)new OkObjectResult(permission);
                //        }

                //        return (IActionResult)new BadRequestObjectResult("UserId parameter is null or empty");
                //    }

                //    return (IActionResult)new BadRequestObjectResult("UserId parameter is missing");
                //}

                // return (IActionResult)new BadRequestObjectResult("Header 'x-zumo-auth' is missing or has no value");

        }
    }
}

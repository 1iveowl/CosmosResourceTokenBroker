using System;
using System.Threading.Tasks;
using CosmosResourceTokenBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            try
            {
                await using var brokerService = new ResourceTokenBrokerService(_hostUrl, _key, _databaseId, _collectionId, log);

                var token = req?.Headers?["x-zumo-auth"];

                if (!string.IsNullOrEmpty(token))
                {

                    if (req.GetQueryParameterDictionary().TryGetValue("userId", out var userId))
                    {
                        if (!string.IsNullOrEmpty(userId.Trim()))
                        {
                            var permission = await brokerService.Get(token, userId);

                            return (IActionResult)new OkObjectResult(permission);
                        }

                        return (IActionResult)new BadRequestObjectResult("UserId parameter is null or empty");
                    }

                    return (IActionResult)new BadRequestObjectResult("UserId parameter is missing");
                }

                return (IActionResult)new BadRequestObjectResult("Header 'x-zumo-auth' is missing or has no value");
            }
            catch (Exception ex)
            {
                return (IActionResult)new BadRequestObjectResult($"Unable to process request. Unhandled exception: {ex}");
            }
        }
    }
}

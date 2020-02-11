//using System;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace AzureFunction.Broker
//{
//    public class ListUsers
//    {
//        private readonly string _hostUrl;
//        private readonly string _key;
//        private readonly string _databaseId;
//        private readonly string _collectionId;

//        public ListUsers()
//        {
//            _hostUrl = Environment.GetEnvironmentVariable("CosmosUrl");
//            _key = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
//            _databaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
//            _collectionId = Environment.GetEnvironmentVariable("CosmosCollection");
//        }

//        [FunctionName("ListUsers")]
//        public  async Task<IActionResult> Run(
//            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "users")] HttpRequest req,
//            ILogger log)
//        {
//            using var httpClient = new HttpClient();
            
//            var resourceType = "dbs";
//            var resourceLink = "dbs"; // $"dbs/{_databaseId.ToLowerInvariant()}";
//            var resourceId = "";

//            var authHeader = GenerateAuthToken("GET", resourceType, resourceId, DateTime.UtcNow.ToString("R"), _key, "master", "1.0");
            
//            httpClient.DefaultRequestHeaders.Clear();

//            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);

//            httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString("R"));
//            httpClient.DefaultRequestHeaders.Add("x-ms-version", "2015-08-06");

//            try
//            {
//                var uri = new Uri($"{_hostUrl}{resourceLink}");

//                var response = await httpClient.GetAsync(uri);

//                if (response.IsSuccessStatusCode)
//                {
//                    var result = response.Content.ReadAsStringAsync();

//                    return new OkObjectResult(result);
//                }

//                return new BadRequestObjectResult($"Error: {response.ReasonPhrase}");
//            }
//            catch (Exception ex)
//            {
//                return new BadRequestObjectResult($"Unhandled exception: {ex}");
//            }
//        }

//        private string GenerateAuthToken(string verb, string resourceType, string resourceId, string date, string key, string keyType, string tokenVersion)
//        {
//            var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };

//            verb = verb ?? "";
//            resourceType = resourceType ?? "";
//            resourceId = resourceId ?? "";

//            string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
//                verb.ToLowerInvariant(),
//                resourceType.ToLowerInvariant(),
//                resourceId,
//                date.ToLowerInvariant(),
//                ""
//            );

//            byte[] hashPayLoad = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payLoad));
//            string signature = Convert.ToBase64String(hashPayLoad);

//            return System.Web.HttpUtility.UrlEncode(String.Format(System.Globalization.CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
//                keyType,
//                tokenVersion,
//                signature));
//        }
//    }
//}

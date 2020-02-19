using System.IO;
using System.Linq;
using System.Reflection;
using CosmosResourceToken.Core.Client;
using CosmosResourceTokenClient;
using Newtonsoft.Json.Linq;
using ResourceTokenBrokerTest.Model;
using ResourceTokenBrokerTest.Service;
using Xunit;

namespace ResourceTokenBrokerTest
{
    public class CosmosTokenClientTest
    {
        private ICosmosTokenClient _cosmosTokenClient;

        public CosmosTokenClientTest()
        {
            const string defaultNameSpace = "ResourceTokenBrokerTest";

            var assembly = typeof(CosmosTokenClientTest).GetTypeInfo().Assembly;

            var fileStream = assembly.GetManifestResourceStream($"{defaultNameSpace}.test.config.json");

            if (!(fileStream is null))
            {
                using var reader = new StreamReader(fileStream);
                var jsonStr = reader.ReadToEnd();

                var configObject = JObject.Parse(jsonStr);

                var resourceTokenBrokerUrl = configObject["ResourceTokenBrokerUrl"].ToString();
                var accessToken = configObject["AccessToken"].ToString();

                var scopes = ((JArray)configObject["Scopes"])?.Select(scope => scope?.ToString());

                // TODO Read the access token and get values...

                var testB2CAuthService = new TestB2CAuthService(new TestUserContext
                {
                    AccessToken =  accessToken,
                    IsLoggedOn = true,
                    UserIdentifier = //From accessToken
                });

                _cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, resourceTokenBrokerUrl);
            }
        }


        [Fact]
        public void Test1()
        {

        }
    }
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using Console.EF.Cosmos.Model;
using Newtonsoft.Json.Linq;

namespace Console.EF.Cosmos
{
    class Program
    {
        private readonly string _randomGuid;

        private readonly string _accessToken;
        private readonly string _expiredAccessToken;
        private readonly string _resourceTokenBrokerUrl;
        private readonly string _resourceTokenBrokerUrlLocalHost;

        public Program()
        {
            const string defaultNameSpace = "ResourceTokenBrokerTest";

            _randomGuid = Guid.NewGuid().ToString();

            var assembly = typeof(Program).GetTypeInfo().Assembly;

            var fileStream = assembly.GetManifestResourceStream($"{defaultNameSpace}.test.config.json");

            if (!(fileStream is null))
            {
                using var reader = new StreamReader(fileStream);
                var jsonStr = reader.ReadToEnd();

                var configObject = JObject.Parse(jsonStr);

                _accessToken = configObject["AccessToken"].ToString();
                _expiredAccessToken = configObject["ExpiredAccessToken"]?.ToString();
                _resourceTokenBrokerUrl = configObject["ResourceTokenBrokerUrl"].ToString();
                _resourceTokenBrokerUrlLocalHost = configObject["ResourceTokenBrokerUrlLocalHost"]?.ToString();

                var scopes = ((JArray)configObject["Scopes"])?.Select(scope => scope?.ToString());
            }
        }


        public static async Task Main(string[] args)
        {
            //System.Console.WriteLine($"Test: {_a}");

        }

        private IUserContext CreateTestUserContext(bool isValidToken, bool isAzureFunctionLocalEmulator)
        {
            var token = isValidToken ? _accessToken : _expiredAccessToken;

            var handler = new JwtSecurityTokenHandler();

            var decryptedToken = handler.ReadJwtToken(token);

            var userIdentifier = decryptedToken.Subject;

            var userContext = new TestUserContext
            {
                AccessToken = token,
                IsLoggedOn = true,
                UserIdentifier = userIdentifier
            };

            return userContext;
        }
    }
}

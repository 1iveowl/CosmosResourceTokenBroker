using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using B2CAuthClient.Abstract;
using Console.EF.Cosmos.Model;
using Newtonsoft.Json.Linq;

namespace Console.EF.Cosmos.Service
{
    public class AuthMock
    {
        private readonly string _randomGuid;

        private readonly string _accessToken;
        private readonly string _expiredAccessToken;
        private readonly string _resourceTokenBrokerUrlLocalHost;

        internal string ResourceTokenBrokerUrl { get; }

        public AuthMock()
        {
            const string defaultNameSpace = "Console.EF.Cosmos";

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
                _resourceTokenBrokerUrlLocalHost = configObject["ResourceTokenBrokerUrlLocalHost"]?.ToString();

                ResourceTokenBrokerUrl = configObject["ResourceTokenBrokerUrl"].ToString();

                var scopes = ((JArray)configObject["Scopes"])?.Select(scope => scope?.ToString());
            }
        }

        internal IUserContext GetUserContext(bool isValidToken = true)
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

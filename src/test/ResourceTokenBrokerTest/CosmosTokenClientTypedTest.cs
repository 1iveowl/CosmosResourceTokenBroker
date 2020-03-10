using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceTokenClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceTokenBrokerTest.Model;
using ResourceTokenBrokerTest.Service;
using Xunit;
using Xunit.Extensions.Ordering;
using Person = ResourceTokenBrokerTest.Model.Person;

namespace ResourceTokenBrokerTest
{
    public class CosmosTokenClientTypedTest
    {
        // Use this switch to swap between using local emulator and Azure Function in the cloud.
        private const bool IsAzureFunctionLocalEmulator = false;

        private readonly string _randomGuid;

        private readonly string _accessToken;
        private readonly string _expiredAccessToken;
        private readonly string _resourceTokenBrokerUrl;
        private readonly string _resourceTokenBrokerUrlLocalHost;
        
        public CosmosTokenClientTypedTest()
        {
            const string defaultNameSpace = "ResourceTokenBrokerTest";

            _randomGuid = Guid.NewGuid().ToString();

            var assembly = typeof(CosmosTokenClientTypedTest).GetTypeInfo().Assembly;

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

        [Theory, Order(100)]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument, "Ian", "Fleming", true)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared, "Ian", "Fleming", false)]
        public async Task ReplaceDocument(bool isValidToken, string documentPrefix, DefaultPartitionKind defaultPartition, string firstName, string lastName, bool result)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documentId = defaultPartition == DefaultPartitionKind.Shared
                ? documentPrefix
                : $"{documentPrefix}-{userContext.UserIdentifier}";

            var person = new Person { FirstName = firstName, LastName = lastName };

            try
            {
                await cosmosTokenClient.Replace(documentId, person, defaultPartition);
                Assert.True(result);
                return;
            }
            catch (CosmosClientException ex)
            {
                Assert.True(ex is CosmosClientException);
            }

            Assert.False(result);
        }
        
        [Theory, Order(200)]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument, "James", "Bond", true, true)]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument, "James", "Bond", false, false)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared, "James", "Bond", false, false)]
        public async Task CreateDocument(
            bool isValidToken, 
            string documentPrefix, 
            DefaultPartitionKind defaultPartition, 
            string firstName, 
            string lastName,
            bool randomizeDocumentId,
            bool result)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documentId = defaultPartition == DefaultPartitionKind.Shared
                ? documentPrefix
                : randomizeDocumentId
                    ? $"{documentPrefix}-007-{userContext.UserIdentifier}"
                    : $"{documentPrefix}-{userContext.UserIdentifier}";

            var person = new Person { FirstName = firstName, LastName = lastName };

            try
            {
                await cosmosTokenClient.Create(documentId, person, defaultPartition);
                Assert.True(result);
                return;
            }
            catch (CosmosClientException ex)
            {
                Assert.True(ex is CosmosClientException);
            }

            Assert.False(result);
        }

        [Theory, Order(300)]
        [InlineData(true,  DefaultPartitionKind.UserDocument, 2)]
        [InlineData(true, DefaultPartitionKind.Shared, 1)]
        public async Task ReadDocumentList(
            bool isValidToken, 
            DefaultPartitionKind defaultPartition, 
            int numberOfDocuments)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documents = await cosmosTokenClient.GetPartitionDocuments(defaultPartition);

            Assert.True( documents.Count() >= numberOfDocuments);
        }

        [Theory, Order(400)]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument,  true)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared, false)]
        public async Task DeleteDocument(
            bool isValidToken,
            string documentPrefix,
            DefaultPartitionKind defaultPartition,
            bool result)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documentId = defaultPartition == DefaultPartitionKind.Shared
                ? documentPrefix
                : $"{documentPrefix}-007-{userContext.UserIdentifier}";

            try
            {
                await cosmosTokenClient.Delete(documentId, defaultPartition);
                Assert.True(result);
                return;
            }
            catch (CosmosClientException ex)
            {
                Assert.True(ex is CosmosClientException);
            }

            Assert.False(result);
        }

        [Theory, Order(450)]
        [InlineData(true, DefaultPartitionKind.UserDocument, 2)]
        [InlineData(true, DefaultPartitionKind.Shared, 1)]
        public async Task GetList(
            bool isValidToken,
            DefaultPartitionKind defaultPartition,
            int numberOfDocuments)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documents = await cosmosTokenClient.List<Person>(defaultPartition);

            Assert.True(documents.Count() >= numberOfDocuments);
        }


        // This test requires that there are two documents in your Cosmos DB of the type 'Person'.
        // The share document should have firstname: John, lastname: Doe
        // The user document should be in a partition that the user of the valid access token has access to.
        // The user document should have firstname: Ian, lastname: Fleming
        [Theory, Order(500)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared, "John", "Doe")]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument, "Ian", "Fleming")]
        public async Task ReadDocument(bool isValidToken, string documentPrefix, DefaultPartitionKind defaultPartition, string expectedFirstName, string expectedLastName)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documentId = defaultPartition == DefaultPartitionKind.Shared
                ? documentPrefix
                : $"{documentPrefix}-{userContext.UserIdentifier}";

            var document = await cosmosTokenClient.Read<Person>(documentId, defaultPartition);

            Assert.Equal(expectedFirstName, document.FirstName);
            Assert.Equal(expectedLastName, document.LastName);
        }

        [Theory, Order(525)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared)]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument)]
        public async Task ReadMissingDocument(bool isValidToken, string documentPrefix, DefaultPartitionKind defaultPartition)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            try
            {
                var document = await cosmosTokenClient.Read<Person>("DoesNotExist", defaultPartition);
            }
            catch (Exception ex)
            {
                Assert.True(ex is DataException);
            }
        }

        // This test requires that there are two documents in your Cosmos DB of the type 'Person'.
        // The share document should have firstname: John, lastname: Doe
        // The user document should be in a partition that the user of the valid access token has access to.
        // The user document should have firstname: Ian, lastname: Fleming
        [Theory, Order(550)]
        [InlineData(true, "Person", DefaultPartitionKind.Shared, "John", "Doe")]
        [InlineData(true, "Person", DefaultPartitionKind.UserDocument, "James", "Bond")]
        public async Task ReadDocumentEx(bool isValidToken, string documentPrefix, DefaultPartitionKind defaultPartition, string expectedFirstName, string expectedLastName)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            var documentId = defaultPartition == DefaultPartitionKind.Shared
                ? documentPrefix
                : $"{documentPrefix}2-{userContext.UserIdentifier}";

            var document = await cosmosTokenClient.Read<PersonEx>(documentId, defaultPartition);

            Assert.Equal(expectedFirstName, document.FirstName);
            Assert.Equal(expectedLastName, document.LastName);
        }



        // Trying to read a document that exist but that has a partition key that user does not have access to.
        [Theory, Order(600)]
        [InlineData(true,  DefaultPartitionKind.Shared, "Person-0000001-0001-0001-0001-000000000001")]
        public async Task TryToReadDocumentThatUserDoesNotHaveAccessTo(bool isValidToken, DefaultPartitionKind defaultPartition, string documentId)
        {
            var brokerUrl = IsAzureFunctionLocalEmulator ? _resourceTokenBrokerUrlLocalHost : _resourceTokenBrokerUrl;

            var userContext = CreateTestUserContext(isValidToken, IsAzureFunctionLocalEmulator);
            var testB2CAuthService = new TestB2CAuthService(userContext);

            await using var cosmosTokenClient = new CosmosTokenClient(testB2CAuthService, brokerUrl);

            try
            {
                var document = await cosmosTokenClient.Read<Person>(documentId, defaultPartition);
            }
            catch (Exception ex)
            {
                Assert.True(ex is DataException);
            }
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

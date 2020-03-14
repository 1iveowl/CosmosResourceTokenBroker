using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;

namespace Console.EF.Cosmos.Client
{
    public class CosmosTokenClientEF : BrokerClientHandler
    {
        private readonly string _resourceTokenBrokerUrl;

        public CosmosTokenClientEF(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl, 
            ICacheSingleObjectByKey resourceTokenCache = null) : base(authService, resourceTokenBrokerUrl, resourceTokenCache)
        {
            _resourceTokenBrokerUrl = resourceTokenBrokerUrl;
        }



    }
}

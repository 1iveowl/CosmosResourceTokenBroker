using System;

namespace CosmosResourceToken.Core.Broker
{
    public class ResourceTokenBrokerServiceException : Exception
    {
        public ResourceTokenBrokerServiceException() { }

        public ResourceTokenBrokerServiceException(string message) : base(message) { } 

        public ResourceTokenBrokerServiceException(string message, Exception ex) : base(message, ex) { }
    }
}

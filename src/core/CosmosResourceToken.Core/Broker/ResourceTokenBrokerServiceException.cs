using System;

namespace CosmosResourceToken.Core.Broker
{
    [Preserve(AllMembers = true)]
    public class ResourceTokenBrokerServiceException : Exception
    {
        public ResourceTokenBrokerServiceException() { }

        public ResourceTokenBrokerServiceException(string message) : base(message) { } 

        public ResourceTokenBrokerServiceException(string message, Exception ex) : base(message, ex) { }
    }
}

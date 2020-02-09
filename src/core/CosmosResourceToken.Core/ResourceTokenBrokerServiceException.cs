using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosResourceToken.Core
{
    public class ResourceTokenBrokerServiceException : Exception
    {
        public ResourceTokenBrokerServiceException() { }

        public ResourceTokenBrokerServiceException(string message) : base(message) { } 

        public ResourceTokenBrokerServiceException(string message, Exception ex) : base(message, ex) { }
    }
}

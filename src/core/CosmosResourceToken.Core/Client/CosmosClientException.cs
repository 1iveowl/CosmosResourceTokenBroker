using System;

namespace CosmosResourceToken.Core.Client
{
    [Preserve(AllMembers = true)]
    public class CosmosClientException : Exception
    {
        public CosmosClientException() { }
               
        public CosmosClientException(string message) : base(message) { }
               
        public CosmosClientException(string message, Exception ex) : base(message, ex) { }
    }

    [Preserve(AllMembers = true)]
    public class CosmosClientAuthenticationException : Exception
    {
        public CosmosClientAuthenticationException() { }

        public CosmosClientAuthenticationException(string message) : base(message) { }

        public CosmosClientAuthenticationException(string message, Exception ex) : base(message, ex) { }

    }
}
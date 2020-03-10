using System;
using System.Collections.Generic;
using System.Text;

namespace B2CAuthClient.Abstract
{
    [Preserve(AllMembers = true)]
    public class B2CAuthClientException : Exception
    {
        public bool InteractiveSignInRequested { get; } = false;

        public B2CAuthClientException() { }
               
        public B2CAuthClientException(string message) : base(message) { }
              
        public B2CAuthClientException(string message, Exception ex) : base(message, ex) { }

        public B2CAuthClientException(bool interactiveSignInRequested, string message, Exception ex) : base(message, ex)
        {
            InteractiveSignInRequested = interactiveSignInRequested;
        }

        public B2CAuthClientException(bool interactiveSignInRequested, string message) : base(message)
        {
            InteractiveSignInRequested = interactiveSignInRequested;
        }
    }
}

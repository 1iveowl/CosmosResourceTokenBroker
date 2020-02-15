using System;
using System.Collections.Generic;
using System.Linq;
using CosmosResourceToken.Core.Client;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using XamarinForms.Client.Authentication.Interface;
using static XamarinForms.Client.Utility.ExtensionMethod;

namespace XamarinForms.Client.Authentication.Model
{
    public class UserContext : IUserContext
    {
        public string Name { get; }
        public string UserIdentifier { get; }
        public bool NewUser { get; }
        public string Scope { get; }
        public bool IsLoggedOn { get; }
        public string GivenName { get; }
        public string FamilyName { get;  }
        public string Province { get; }
        public string PostalCode { get; }
        public string Country { get; }
        public IEnumerable<string> EmailAddresses { get;  }
        public string JobTitle { get; }
        public string StreetAddress { get; }
        public string City { get; }
        public string AccessToken { get; }

        internal UserContext(AuthenticationResult authResult)
        {
            if (authResult is null)
            {
                throw new ArgumentNullException();
            }

            AccessToken = authResult.AccessToken;

            if (string.IsNullOrEmpty(AccessToken.Trim()))
            {
                throw new ArgumentException("Unable to create UserContext. Object 'Access Token' in AuthenticationResult is missing or invalid");
            }

            var jwtPayload = authResult.ToJwtPayloadAsJObject();

            if (jwtPayload is null)
            {
                throw new ArgumentException("Unable to create UserContext. Object 'User' now defined in AuthenticationResult");
            }

            NewUser = jwtPayload["newUser"]?.ToString()?.ToLowerInvariant() == "true";
            Scope = jwtPayload["scp"]?.ToString();
            Name = jwtPayload["user"]?.ToString();
            UserIdentifier = jwtPayload["sub"]?.ToString();
            GivenName = jwtPayload["given_name"]?.ToString();
            FamilyName = jwtPayload["family_name"]?.ToString();
            StreetAddress = jwtPayload["streetAddress"]?.ToString();
            City = jwtPayload["city"]?.ToString();
            Province = jwtPayload["state"]?.ToString();
            PostalCode = jwtPayload["postalCode"]?.ToString();
            Country = jwtPayload["country"]?.ToString();
            JobTitle = jwtPayload["jobTitle"]?.ToString();

            EmailAddresses = ((JArray)jwtPayload["emails"])?.Select(emailAddress => emailAddress?.ToString());

            IsLoggedOn = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using B2CAuthClient.Abstract;
using B2CAuthClient.Utility;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

namespace B2CAuthClient
{
    public class UserContext : IUserContext
    {
        public bool IsLoggedOn { get; }
        public string Name { get; }
        public string UserIdentifier { get; }
        public bool IsNewUser { get; }
        public string Scope { get; }
        public string GivenName { get; }
        public string FamilyName { get;  }
        public string Province { get; }
        public string PostalCode { get; }
        public string Country { get; }
        public IEnumerable<string> EmailAddresses { get; }
        public string JobTitle { get; }
        public string StreetAddress { get; }
        public string City { get; }
        public string AccessToken { get; }
        public DateTime AccessTokenExpires { get; }
        public bool HasAccessTokenExpired => AccessTokenExpires < DateTime.UtcNow;

        internal UserContext()
        {
            IsLoggedOn = false;
        }

        internal UserContext(IUserContext userContext, bool isLoggedOn)
        {
            if (userContext is null)
            {
                return;
            }

            IsLoggedOn = isLoggedOn;
            Name = userContext.Name;
            UserIdentifier = userContext.UserIdentifier;
            IsNewUser = userContext.IsNewUser;
            Scope = userContext.Scope;
            GivenName = userContext.GivenName;
            FamilyName = userContext.FamilyName;
            Province = userContext.Province;
            PostalCode = userContext.PostalCode;
            Country = userContext.Country;
            EmailAddresses = userContext.EmailAddresses;
            JobTitle = userContext.JobTitle;
            StreetAddress = userContext.StreetAddress;
            City = userContext.City;
            AccessToken = userContext.AccessToken;
            AccessTokenExpires = userContext.AccessTokenExpires;
        }

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

            IsNewUser = jwtPayload["newUser"]?.ToString()?.ToLowerInvariant() == "true";
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

            if (double.TryParse(jwtPayload["exp"]?.ToString(), out var expiresEpoch))
            {
                AccessTokenExpires = expiresEpoch.UnixTimeStampToDateTimeUtc();
            }

            EmailAddresses = ((JArray)jwtPayload["emails"])?.Select(emailAddress => emailAddress?.ToString());

            IsLoggedOn = true;
        }
    }
}

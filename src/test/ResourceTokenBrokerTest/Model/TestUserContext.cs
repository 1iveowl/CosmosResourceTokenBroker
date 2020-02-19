using System;
using System.Collections.Generic;
using System.Text;
using B2CAuthClient.Abstract;

namespace ResourceTokenBrokerTest.Model
{
    public class TestUserContext : IUserContext
    {
        public bool IsLoggedOn { get; set; }
        public string Name { get; set; }
        public string UserIdentifier { get; set; }
        public bool NewUser { get; set; }
        public string Scope { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public IEnumerable<string> EmailAddresses { get; set; }
        public string JobTitle { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpires { get; }
        public bool HasAccessTokenExpired => AccessTokenExpires < DateTime.UtcNow;
    }
}

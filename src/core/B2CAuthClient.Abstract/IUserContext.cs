using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace B2CAuthClient.Abstract
{
    [Preserve(AllMembers = true)]
    public interface IUserContext
    {
        bool IsLoggedOn { get; }
        string Name { get; }
        string UserIdentifier { get; }
        bool NewUser { get; }
        string Scope { get; }
        string GivenName { get;}
        string FamilyName { get;}
        string Province { get; }
        string PostalCode { get; }
        string Country { get; } IEnumerable<string> EmailAddresses { get; }
        string JobTitle { get; }
        string StreetAddress { get; }
        string City { get; }
        string AccessToken { get;}
        DateTime AccessTokenExpires { get; }
        bool HasAccessTokenExpired { get; }
    }
}

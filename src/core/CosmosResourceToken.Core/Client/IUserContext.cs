using System.Collections.Generic;

namespace CosmosResourceToken.Core.Client
{
    public interface IUserContext
    {
        string Name { get; }
        string UserIdentifier { get; }
        bool NewUser { get; }
        string Scope { get; }
        bool IsLoggedOn { get;}
        string GivenName { get;}
        string FamilyName { get;}
        string Province { get; }
        string PostalCode { get; }
        string Country { get; } IEnumerable<string> EmailAddresses { get; }
        string JobTitle { get; }
        string StreetAddress { get; }
        string City { get; }
        string AccessToken { get;}
    }
}

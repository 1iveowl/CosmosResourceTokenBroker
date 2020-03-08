using System;
using System.Collections.Generic;

namespace B2CAuthClient.Abstract
{
    [Preserve(AllMembers = true)]
    public interface IUserContext
    {
        /// <summary>
        ///     <para>
        ///         Is <c>true</c> if the user is logged in.
        ///     </para>
        /// </summary>
        bool IsLoggedOn { get; }

        /// <summary>
        ///     <para>
        ///         Is <c>true</c> if the user is a new user that has just been created.
        ///     </para>
        /// </summary>
        bool IsNewUser { get; }

        /// <summary>
        ///     <para>
        ///         The access token of the logged in user.
        ///     </para>
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        ///     <para>
        ///         The date and time for the expiration of the access token.
        ///     </para>
        /// </summary>
        DateTime AccessTokenExpires { get; }

        /// <summary>
        ///     <para>
        ///         Is <c>true</c> if the access token has expired.
        ///     </para>
        /// </summary>
        bool HasAccessTokenExpired { get; }

        /// <summary>
        ///     <para>
        ///         The unique identification id of the user (guid).
        ///     </para>
        ///     <para>
        ///         Provided by Azure AD B2C.
        ///     </para>
        /// </summary>
        string UserIdentifier { get; }

        /// <summary>
        ///     <para>
        ///         User name - typically email.
        ///     </para>
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     <para>
        ///         Scope.
        ///     </para>
        /// </summary>
        string Scope { get; }

        /// <summary>
        ///     <para>
        ///         Given name.
        ///     </para>
        /// </summary>
        string GivenName { get;}

        /// <summary>
        ///     <para>
        ///         Family name.
        ///     </para>
        /// </summary>
        string FamilyName { get;}

        /// <summary>
        ///     <para>
        ///         Province
        ///     </para>
        /// </summary>
        string Province { get; }

        /// <summary>
        ///     <para>
        ///         Postal code.
        ///     </para>
        /// </summary>
        string PostalCode { get; }

        /// <summary>
        ///     <para>
        ///         Country
        ///     </para>
        /// </summary>
        string Country { get; }

        /// <summary>
        ///     <para>
        ///         Job title
        ///     </para>
        /// </summary>
        string JobTitle { get; }

        /// <summary>
        ///     <para>
        ///         Street address
        ///     </para>
        /// </summary>
        string StreetAddress { get; }

        /// <summary>
        ///     <para>
        ///         City
        ///     </para>
        /// </summary>
        string City { get; }

        /// <summary>
        ///     <para>
        ///         List of emails.
        ///     </para>
        /// </summary>
        IEnumerable<string> EmailAddresses { get; }
    }
}

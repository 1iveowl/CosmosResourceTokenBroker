using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2CAuthClient.Abstract
{
    /// <summary>
    ///     <para>
    ///         A simple interface for implementing log-in and log-out of Azure AD B2C authenticated user and getting current state of a logged in user..
    ///     </para>
    /// </summary>

    [Preserve(AllMembers = true)]
    public interface IB2CAuthService
    {
        /// <summary>
        ///     <para>
        ///         Get the current user context of a log-in user.
        ///     </para>
        ///     <para>
        ///         Null if no user has signed in.
        ///     </para>
        /// </summary>
        IUserContext CurrentUserContext { get; }

        /// <summary>
        ///     <para>
        ///         Sign-in user.
        ///     </para>
        /// </summary>
        /// <param name="scopes">The list of scopes used when logging in.</param>
        /// <param name="silentlyOnly">When set to true, only get user context from cache, do not try interactive log-in.</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The user context of the logged-in user.</returns>

        Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Sign-out user.
        ///     </para> 
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task SignOut(CancellationToken cancellationToken = default);
    }
}

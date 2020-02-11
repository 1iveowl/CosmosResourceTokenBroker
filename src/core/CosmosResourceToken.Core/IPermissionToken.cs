using System;

namespace CosmosResourceToken.Core
{
    public interface IPermissionToken
    {
        string Id { get; }
        string Token { get; }
        DateTime ExpiresUtc { get; }
        string UserId { get; }
    }
}

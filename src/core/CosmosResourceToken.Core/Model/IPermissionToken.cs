using System;

namespace CosmosResourceToken.Core.Model
{
    public interface IPermissionToken
    {
        string Id { get; }
        string Token { get; }
        DateTime ExpiresUtc { get; }
        string UserId { get; }
        string EndpointUrl { get; }
    }
}

namespace CosmosResourceToken.Core
{
    public interface IPermissionToken
    {
        string Id { get; }
        string Token { get; }
        int Expires { get; }
        string UserId { get; }
    }
}

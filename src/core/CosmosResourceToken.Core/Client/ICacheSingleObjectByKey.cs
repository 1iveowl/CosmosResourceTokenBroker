using System;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    public interface ICacheSingleObjectByKey<T>
    {
        Task<(CacheObjectStateKind cacheState, T obj)> TryGetFromCache(string key);

        Task CacheObject(T obj, string key, DateTime expiresUtc);
    }
}

using System;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    public interface ICacheSingleObjectByKey
    {
        Task<T> TryGetFromCache<T>(
            string key,
            Func<Task<T>> renewObjectFunc,
            Func<T, Task<bool>> isCachedObjectValidFunc);

        Task CacheObject<T>(T obj, string key);
    }
}

using System;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    /// <summary>
    ///     <para>
    ///         Interface for caching a single object by key.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface ICacheSingleObjectByKey
    {
        /// <summary>
        ///     <para>
        ///         Try get object from cache based on ky.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="key">Object key.</param>
        /// <param name="renewObjectFunc">Func to be used to get the object if it's not in the cache.</param>
        /// <param name="isCachedObjectValidFunc">Func determining if the object in the cache is valid.</param>
        /// <returns></returns>
        Task<T> TryGetFromCache<T>(
            string key,
            Func<Task<T>> renewObjectFunc,
            Func<T, Task<bool>> isCachedObjectValidFunc);

        /// <summary>
        ///     <para>
        ///         Place object in cache.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="key">Key for the object.</param>
        /// <returns></returns>
        Task CacheObject<T>(T obj, string key);
    }
}

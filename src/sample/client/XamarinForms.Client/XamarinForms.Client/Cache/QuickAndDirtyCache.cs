using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;

namespace XamarinForms.Client.Cache
{
    // This is for demo purposes. You should probably consider something like https://github.com/reactiveui/Akavache
    // or https://github.com/jamesmontemagno/monkey-cache to be used here.
    public class QuickAndDirtyCache<T>: ICacheSingleObjectByKey<T> where T : class, new()
    {
        private readonly IDictionary<string, (T cacheObj, DateTime expires)> _cacheDictionary;


        public QuickAndDirtyCache()
        {
            _cacheDictionary = new ConcurrentDictionary<string, (T obj, DateTime expires)>();
        }

        public async Task<(CacheObjectStateKind cacheState, T obj)> TryGetFromCache(string key) 
        {
            if (_cacheDictionary.TryGetValue(key, out var cacheObj))
            {
                var (obj, expires) = cacheObj;

                return expires < DateTime.UtcNow ? (CacheObjectStateKind.Ok, obj) : (CacheObjectStateKind.Expired, obj);
            }

            await Task.CompletedTask;

            return (CacheObjectStateKind.Missing, default);
        }

        public async Task CacheObject(T obj, string key, DateTime expiresUtc)
        {
            _cacheDictionary.Add(key, (obj, expiresUtc));

            await Task.CompletedTask;
        }
    }
}

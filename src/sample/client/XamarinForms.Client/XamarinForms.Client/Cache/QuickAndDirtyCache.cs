using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;

namespace XamarinForms.Client.Cache
{
    // This is for demo purposes. You should probably consider something like https://github.com/reactiveui/Akavache
    // or https://github.com/jamesmontemagno/monkey-cache to be used here.
    public class QuickAndDirtyCache: ICacheSingleObjectByKey
    {
        private readonly IDictionary<string, object> _cacheDictionary;


        public QuickAndDirtyCache()
        {
            _cacheDictionary = new ConcurrentDictionary<string, object>();
        }

        public async Task<T> TryGetFromCache<T>(
            string key, 
            Func<Task<T>> renewObjectFunc,
            Func<T, Task<bool>> isCachedObjectValidFunc)
        {
            if (_cacheDictionary.TryGetValue(key, out var cachedObj))
            {
                var obj = (T) cachedObj;

                if (await isCachedObjectValidFunc(obj))
                {
                    return obj;
                }
            }

            var newObj = await renewObjectFunc();

            await CacheObject(newObj, key);

            return newObj;

        }

        public async Task CacheObject<T>(T obj, string key)
        {
            if (_cacheDictionary.ContainsKey(key))
            {
                _cacheDictionary.Remove(key);
            }

            _cacheDictionary.Add(key, obj);

            await Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.Boilerplate.Caching.Default
{
    public class CacheManager : ICacheManager, IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _createLocks = new ConcurrentDictionary<string, SemaphoreSlim>();


        public CacheManager(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value ?? new CacheOptions();
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, Func<T, bool> shouldCache = null)
        {
            if (TryGet(key, out T entry))
            {
                return entry;
            }

            var entryLock = _createLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            try
            {
                await entryLock.WaitAsync();

                if (TryGet(key, out entry))
                {
                    return entry;
                }

                var value = await valueFactory();

                // Decide if the value should be cached based on the response
                if (shouldCache != null && !shouldCache(value))
                {
                    return value;
                }

                // Set different timeout for stale content
                var valueCacheOptions = new MemoryCacheEntryOptions();
                if (value is AbstractResponse ar && ar.HasStaleContent)
                {
                    valueCacheOptions.SetAbsoluteExpiration(_cacheOptions.StaleContentExpiration);
                }
                else
                {
                    valueCacheOptions.SetAbsoluteExpiration(_cacheOptions.DefaultExpiration);
                }

                return _memoryCache.Set(key, value, valueCacheOptions);
            }
            finally
            {
                entryLock.Release();
            }


        }

        public bool TryGet<T>(string key, out T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _memoryCache.TryGetValue(key, out value);
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
        }
    }
}
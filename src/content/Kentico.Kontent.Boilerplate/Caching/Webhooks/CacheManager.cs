using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Kentico.Kontent.Boilerplate.Caching.Webhooks
{
    public class CacheManager : ICacheManager, IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _createLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly ConcurrentDictionary<string, object> _dependencyLocks = new ConcurrentDictionary<string, object>();


        public CacheManager(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value ?? new CacheOptions();
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, Func<T, IEnumerable<string>> dependenciesFactory, Func<T, bool> shouldCache = null)
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
                    valueCacheOptions.SetSlidingExpiration(_cacheOptions.DefaultExpiration);
                }

                var dependencies = dependenciesFactory?.Invoke(value) ?? new List<string>();
                var dependencyCacheOptions = new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove };
                foreach (var dependency in dependencies)
                {
                    var dependencyKey = dependency;
                    var dependencyLock = _dependencyLocks.GetOrAdd(dependencyKey, _ => new object());

                    if (!_memoryCache.TryGetValue(dependencyKey, out CancellationTokenSource tokenSource) || tokenSource.IsCancellationRequested)
                    {
                        lock (dependencyLock)
                        {
                            if (!_memoryCache.TryGetValue(dependencyKey, out tokenSource) || tokenSource.IsCancellationRequested)
                            {
                                tokenSource = _memoryCache.Set(dependencyKey, new CancellationTokenSource(), dependencyCacheOptions);
                            }
                        }
                    }

                    if (tokenSource != null)
                    {
                        valueCacheOptions.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
                    }
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

        public void InvalidateDependency(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (TryGet(key, out CancellationTokenSource tokenSource))
            {
                tokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
        }
    }
}
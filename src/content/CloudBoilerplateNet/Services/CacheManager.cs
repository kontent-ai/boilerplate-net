using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public class ReactiveCacheManager : ICacheManager
    {
        #region "Fields"

        private bool _disposed = false;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebhookObservableProvider _webhookObservableProvider;

        #endregion

        #region "Properties"

        public int CacheExpirySeconds
        {
            get;
            set;
        }

        #endregion

        #region "Constructors"

        public ReactiveCacheManager(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache, IWebhookObservableProvider webhookObservableProvider)
        {
            if (projectOptions == null)
            {
                throw new ArgumentNullException(nameof(projectOptions));
            }

            CacheExpirySeconds = projectOptions.Value.CacheTimeoutSeconds;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _webhookObservableProvider = webhookObservableProvider ?? throw new ArgumentNullException(nameof(webhookObservableProvider));
            _webhookObservableProvider.GetObservable().Subscribe((args) => InvalidateEntry(args.RelatedTypesResolver, args.IdentifierSet));
        }

        #endregion

        #region "Public methods"

        public async Task<T> GetOrCreateAsync<T>(IEnumerable<string> identifierTokens, Func<Task<T>> valueFactory, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory)
        {
            // Check existence of the cache entry.
            if (!_memoryCache.TryGetValue(StringHelpers.Join(identifierTokens), out T entry))
            {
                // If it doesn't exist, get it via valueFactory.
                T response = await valueFactory();

                // Create it. (Could be off-loaded to a background thread.)
                CreateEntry(identifierTokens, response, dependencyListFactory);

                return response;
            }

            return entry;
        }

        public void CreateEntry<T>(IEnumerable<string> identifierTokens, T value, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory)
        {
            var dependencies = dependencyListFactory(value);

            // Restart entries' expiration period each time they're requested.
            var entryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(CacheExpirySeconds));

            // Dummy entries never expire.
            var dummyOptions = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);

            foreach (var dependency in dependencies)
            {
                var dummyIdentifierTokens = new List<string> { "dummy", dependency.Type, dependency.Codename };
                var dummyKey = StringHelpers.Join(dummyIdentifierTokens);

                // Dummy entries hold just the CancellationTokenSource, nothing else.
                CancellationTokenSource dummyEntry;

                if (!_memoryCache.TryGetValue(dummyKey, out dummyEntry) || _memoryCache.TryGetValue(dummyKey, out dummyEntry) && dummyEntry.IsCancellationRequested)
                {
                    dummyEntry = _memoryCache.Set(dummyKey, new CancellationTokenSource(), dummyOptions);
                }

                if (dummyEntry != null)
                {
                    // Subscribe the main entry to dummy entry's cancellation token.
                    entryOptions.AddExpirationToken(new CancellationChangeToken(dummyEntry.Token));
                }
            }

            _memoryCache.Set(StringHelpers.Join(identifierTokens), value, entryOptions);
        }

        public void InvalidateEntry(IRelatedTypesResolver relatedTypeResolver, IdentifierSet identifiers)
        {
            if (relatedTypeResolver == null)
            {
                throw new ArgumentNullException(nameof(relatedTypeResolver));
            }

            if (identifiers == null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            foreach (var typeIdentifier in relatedTypeResolver.GetRelatedTypes(identifiers.Type))
            {
                if (_memoryCache.TryGetValue(StringHelpers.Join("dummy", typeIdentifier, identifiers.Codename), out CancellationTokenSource dummyEntry))
                {
                    // Mark all subscribers to the CancellationTokenSource as invalid.
                    dummyEntry.Cancel();
                }
            }
        }

        /// <summary>
        /// The <see cref="IDisposable.Dispose"/> implementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region "Non-public methods"

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _memoryCache.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
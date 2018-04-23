using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

        private readonly IDependentTypesResolver _relatedTypesResolver;
        private bool _disposed;

        #endregion

        #region "Properties"

        public int CacheExpirySeconds
        {
            get;
            set;
        }

        public IMemoryCache MemoryCache { get; }

        #endregion

        #region "Constructors"

        public ReactiveCacheManager(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache, IDependentTypesResolver relatedTypesResolver, IWebhookListener kenticoCloudWebhookListener)
        {
            CacheExpirySeconds = projectOptions?.Value?.CacheTimeoutSeconds ?? throw new ArgumentNullException(nameof(projectOptions));
            MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _relatedTypesResolver = relatedTypesResolver ?? throw new ArgumentNullException(nameof(relatedTypesResolver));

            KenticoCloudWebhookObservableFactory
                .GetObservable(kenticoCloudWebhookListener, nameof(kenticoCloudWebhookListener.WebhookNotification))
                .Subscribe((args) => InvalidateEntry(args.IdentifierSet));
        }

        #endregion

        #region "Public methods"

        public async Task<T> GetOrCreateAsync<T>(IEnumerable<string> identifierTokens, Func<Task<T>> valueFactory, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory)
        {
            // Check existence of the cache entry.
            if (!MemoryCache.TryGetValue(StringHelpers.Join(identifierTokens), out T entry))
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
            var dependencies = dependencyListFactory(value) ?? new List<IdentifierSet>();

            // Restart entries' expiration period each time they're requested.
            var entryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(CacheExpirySeconds));

            // Dummy entries never expire.
            var dummyOptions = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);

            foreach (var dependency in dependencies)
            {
                var dummyIdentifierTokens = new List<string> { KenticoCloudCacheHelper.DUMMY_IDENTIFIER, dependency.Type, dependency.Codename };
                var dummyKey = StringHelpers.Join(dummyIdentifierTokens);

                // Dummy entries hold just the CancellationTokenSource, nothing else.
                CancellationTokenSource dummyEntry;

                if (!MemoryCache.TryGetValue(dummyKey, out dummyEntry) || MemoryCache.TryGetValue(dummyKey, out dummyEntry) && dummyEntry.IsCancellationRequested)
                {
                    dummyEntry = MemoryCache.Set(dummyKey, new CancellationTokenSource(), dummyOptions);
                }

                if (dummyEntry != null)
                {
                    // Subscribe the main entry to dummy entry's cancellation token.
                    entryOptions.AddExpirationToken(new CancellationChangeToken(dummyEntry.Token));
                }
            }

            MemoryCache.Set(StringHelpers.Join(identifierTokens), value, entryOptions);
        }

        public bool TryGetValue<T>(IEnumerable<string> identifierTokens, out T value)
            where T : class
        {
            if (MemoryCache.TryGetValue(StringHelpers.Join(identifierTokens), out T entry))
            {
                value = entry;

                return true;
            }
            else
            {
                value = null;

                return false;
            }
        }

        public void InvalidateEntry(IdentifierSet identifiers)
        {
            if (_relatedTypesResolver == null)
            {
                throw new ArgumentNullException(nameof(_relatedTypesResolver));
            }

            if (identifiers == null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            foreach (var typeIdentifier in _relatedTypesResolver.GetDependentTypeNames(identifiers.Type))
            {
                if (MemoryCache.TryGetValue(StringHelpers.Join(KenticoCloudCacheHelper.DUMMY_IDENTIFIER, typeIdentifier, identifiers.Codename), out CancellationTokenSource dummyEntry))
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
                MemoryCache.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
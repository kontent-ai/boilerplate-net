using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Resolvers;
using System.Collections.Concurrent;

namespace CloudBoilerplateNet.Services
{
    public class ReactiveCacheManager : ICacheManager
    {
        #region "Fields"

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IDependentTypesResolver _dependentTypesResolver;
        private readonly ConcurrentDictionary<string, object> _cacheDummyLocks = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _cacheLocks = new ConcurrentDictionary<string, object>();
        private readonly object _dummyEntryCreationLock = new object();
        private readonly object _entryCreationLock = new object();
        private bool _disposed;

        #endregion

        #region "Properties"

        public int CacheExpirySeconds
        {
            get;
            set;
        }

        public IMemoryCache MemoryCache { get; }

        protected List<string> InvalidatingOperations => new List<string>
        {
            "upsert",
            "publish",
            "restore_publish",
            "unpublish",
            "archive",
            "restore"
        };

        #endregion

        #region "Constructors"

        public ReactiveCacheManager(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache, IDependentTypesResolver relatedTypesResolver, IWebhookListener webhookListener)
        {
            CacheExpirySeconds = projectOptions?.Value?.CacheTimeoutSeconds ?? throw new ArgumentNullException(nameof(projectOptions));
            MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _dependentTypesResolver = relatedTypesResolver ?? throw new ArgumentNullException(nameof(relatedTypesResolver));

            WebhookObservableFactory
                .GetObservable(webhookListener, nameof(webhookListener.WebhookNotification))
                .Where(args => InvalidatingOperations.Any(operation => operation.Equals(args.Operation, StringComparison.Ordinal)))
                .Throttle(TimeSpan.FromSeconds(1))
                .DistinctUntilChanged()
                .Subscribe((args) => InvalidateEntry(args.IdentifierSet));
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Gets an existing cache entry or creates one using the supplied <paramref name="valueFactory"/>.
        /// </summary>
        /// <typeparam name="T">Type of the cache entry value.</typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry.</param>
        /// <param name="valueFactory">Method to create the entry.</param>
        /// <param name="dependencyListFactory">Method to get a collection of identifiers of entries that the current entry depends upon.</param>
        /// <returns>The cache entry value, either cached or obtained through the <paramref name="valueFactory"/>.</returns>
        public async Task<T> GetOrCreateAsync<T>(IEnumerable<string> identifierTokens, Func<Task<T>> valueFactory, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory, bool createCacheEntriesInBackground = false)
        {
            var joinedTokens = StringHelpers.Join(identifierTokens);
            await _semaphoreSlim.WaitAsync();

            try
            {
                // Check existence of the cache entry.
                if (!MemoryCache.TryGetValue(joinedTokens, out T entry))
                {
                    // If it doesn't exist, get it via valueFactory.
                    T response = await valueFactory();

                    // Create it in a background thread.
                    if (createCacheEntriesInBackground)
                    {
                        var task = Task.Run(() => CreateEntry(identifierTokens, response, dependencyListFactory));
                    }
                    else
                    {
                        CreateEntry(identifierTokens, response, dependencyListFactory);
                    }

                    return response;
                }

                return entry;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Creates a new cache entry.
        /// </summary>
        /// <typeparam name="T">Type of the cache entry value.</typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry.</param>
        /// <param name="value">Value of the entry.</param>
        /// <param name="dependencyListFactory">Method to get a collection of identifier of entries that the current entry depends upon.</param>
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
                var newDummyLock = new object();
                object dummyPadlock;

                if (_cacheDummyLocks.TryAdd(dummyKey, newDummyLock))
                {
                    dummyPadlock = newDummyLock;
                }
                else
                {
                    dummyPadlock = _cacheDummyLocks[dummyKey];
                }

                // Dummy entries hold just the CancellationTokenSource, nothing else.
                CancellationTokenSource dummyEntry;

                if (!DummyEntryExists(dummyKey, out dummyEntry))
                {
                    lock (dummyPadlock)
                    {
                        dummyEntry = GetOrCreateDummyEntry(dummyOptions, dummyKey);
                    }
                }

                if (dummyEntry != null)
                {
                    // Subscribe the main entry to dummy entry's cancellation token.
                    entryOptions.AddExpirationToken(new CancellationChangeToken(dummyEntry.Token));
                }
            }

            var key = StringHelpers.Join(identifierTokens);
            var newLock = new object();
            object padlock;

            if (_cacheLocks.TryAdd(key, newLock))
            {
                padlock = newLock;
            }
            else
            {
                padlock = _cacheLocks[key];
            }

            if (!EntryExists(key))
            {
                lock (_entryCreationLock)
                {
                    if (!EntryExists(key))
                    {
                        MemoryCache.Set(StringHelpers.Join(identifierTokens), value, entryOptions);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get a cache entry.
        /// </summary>
        /// <typeparam name="T">Type of the entry.</typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry.</param>
        /// <param name="value">The cache entry value, if it exists.</param>
        /// <returns>True if the entry exists, otherwise false.</returns>
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

        /// <summary>
        /// Invalidates (clears) a cache entry.
        /// </summary>
        /// <param name="identifiers">Identifiers of the entry.</param>
        public void InvalidateEntry(IdentifierSet identifiers)
        {
            if (_dependentTypesResolver == null)
            {
                throw new ArgumentNullException(nameof(_dependentTypesResolver));
            }

            if (identifiers == null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            foreach (var typeIdentifier in _dependentTypesResolver.GetDependentTypeNames(identifiers.Type))
            {
                if (MemoryCache.TryGetValue(StringHelpers.Join(KenticoCloudCacheHelper.DUMMY_IDENTIFIER, typeIdentifier, identifiers.Codename), out CancellationTokenSource dummyEntry))
                {
                    // Mark all subscribers to the CancellationTokenSource as invalid.
                    dummyEntry.Cancel();
                }
            }
        }

        /// <summary>
        /// Looks up the cache for an entry and passes it to a method that extracts specific dependencies.
        /// </summary>
        /// <typeparam name="T">Type of the cache entry.</typeparam>
        /// <param name="identifierSet">Identifiers used to look up the cache for the entry.</param>
        /// <param name="dependencyListFactory">The method that takes the entry, and uses them to extract dependencies from it.</param>
        /// <returns>Identifiers of the dependencies.</returns>
        public IEnumerable<IdentifierSet> GetDependenciesByName<T>(IdentifierSet identifierSet, Func<T, IEnumerable<IdentifierSet>> dependencyFactory)
            where T : class
        {
            var dependencies = new List<IdentifierSet>();

            if (TryGetValue(new[] { identifierSet.Type, identifierSet.Codename }, out T cacheEntry))
            {
                return dependencyFactory(cacheEntry);
            }

            return dependencies;
        }

        /// <summary>
        /// Prepares identifier sets for all dependent types (formats) of the cache entry and passes them onto <paramref name="dependencyFactory"/> to extract specific dependencies.
        /// </summary>
        /// <param name="originalTypeIdentifier">The original type of the cache entry.</param>
        /// <param name="codename">The code name of the cache entry.</param>
        /// <param name="dependencyListFactory">The method that takes each of the identifiers of the dependent types (formats), and uses them to extract dependencies.</param>
        /// <returns>Identifiers of the dependencies.</returns>
        public IEnumerable<IdentifierSet> GetDependenciesByType(string originalTypeIdentifier, string codename, Func<IdentifierSet, IEnumerable<IdentifierSet>> dependencyFactory)
        {
            var dependencies = new List<IdentifierSet>();

            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(originalTypeIdentifier) && dependencyFactory != null)
            {
                foreach (var typeIdentifier in _dependentTypesResolver.GetDependentTypeNames(originalTypeIdentifier))
                {
                    dependencies.AddRange(dependencyFactory(new IdentifierSet { Type = typeIdentifier, Codename = codename }));
                }
            }

            return dependencies;
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

        protected bool EntryExists(string key)
        {
            return MemoryCache.TryGetValue(key, out object existingValue);
        }

        protected CancellationTokenSource GetOrCreateDummyEntry(MemoryCacheEntryOptions dummyOptions, string dummyKey)
        {
            if (!DummyEntryExists(dummyKey, out CancellationTokenSource dummyEntry))
            {
                dummyEntry = MemoryCache.Set(dummyKey, new CancellationTokenSource(), dummyOptions);
            }

            return dummyEntry;
        }

        protected bool DummyEntryExists(string dummyKey, out CancellationTokenSource dummyEntry)
        {
            return MemoryCache.TryGetValue(dummyKey, out dummyEntry) && !dummyEntry.IsCancellationRequested;
        }

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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface ICacheManager : IDisposable
    {
        /// <summary>
        /// Either fixed or floating period of time required for an entry to expire.
        /// </summary>
        int CacheExpirySeconds { get; set; }

        /// <summary>
        /// Gets an existing cache entry or creates one using the supplied <paramref name="valueFactory"/>.
        /// </summary>
        /// <typeparam name="T">Type of the cache entry value.</typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry.</param>
        /// <param name="valueFactory">Method to create the entry.</param>
        /// <param name="dependencyListFactory">Method to get a collection of identifiers of entries that the current entry depends upon.</param>
        /// <returns>The cache entry value, either cached or obtained through the <paramref name="valueFactory"/>.</returns>
        Task<T> GetOrCreateAsync<T>(IEnumerable<string> identifierTokens, Func<Task<T>> valueFactory, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory);

        /// <summary>
        /// Tries to get a cache entry.
        /// </summary>
        /// <typeparam name="T">Type of the entry.</typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry.</param>
        /// <param name="value">The cache entry value, if it exists.</param>
        /// <returns>True if the entry exists, otherwise false.</returns>
        bool TryGetValue<T>(IEnumerable<string> identifierTokens, out T value)
            where T : class;

        /// <summary>
        /// Invalidates (clears) a cache entry.
        /// </summary>
        /// <param name="identifiers">Identifiers of the entry.</param>
        void InvalidateEntry(IdentifierSet identifiers);

        /// <summary>
        /// Looks up the cache for an entry and passes it to a method to extract dependencies.
        /// </summary>
        /// <typeparam name="T">Type of the cache entry.</typeparam>
        /// <param name="typeIdentifier">Type used to look up the cache entry.</param>
        /// <param name="codename">The code name used to look up the cache entry.</param>
        /// <param name="dependencyListFactory">The method that takes the entry and extracts dependencies from it.</param>
        /// <returns>Identifiers of the dependencies.</returns>
        IEnumerable<IdentifierSet> GetDependenciesFromCacheContents<T>(string typeIdentifier, string codename, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory)
            where T : class;

        /// <summary>
        /// Creates identifier sets for all alternative types (formats) of the cache entry.
        /// </summary>
        /// <param name="originalTypeIdentifier">The original type of the cache entry.</param>
        /// <param name="codename">The code name of the cache entry.</param>
        /// <param name="dependencyListFactory">The method that takes each of the type identifiers and the code name, and, extracts dependencies from it.</param>
        /// <returns>Identifiers of the dependencies.</returns>
        IEnumerable<IdentifierSet> GetDependenciesForAllDependentTypes(string originalTypeIdentifier, string codename, Func<string, string, IEnumerable<IdentifierSet>> dependencyListFactory);
    }
}
using KenticoCloud.Delivery;
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
        /// <typeparam name="T">Type of the cache entry value that implements <see cref="IContentItemBase"/></typeparam>
        /// <param name="identifierTokens">String tokens that form a unique identifier of the entry</param>
        /// <param name="valueFactory">Method to create the entry</param>
        /// <param name="dependencyListFactory">Method to get a collection of identifiers of entries that the current entry depends upon</param>
        /// <returns>The value, either cached or obtained through the <paramref name="valueFactory"/>.</returns>
        Task<T> GetOrCreateAsync<T>(IEnumerable<string> identifierTokens, Func<Task<T>> valueFactory, Func<T, IEnumerable<IdentifierSet>> dependencyListFactory);
        
        /// <summary>
        /// Invalidates (clears) an entry.
        /// </summary>
        /// <param name="identifiers">Identifiers of the entry</param>
        void InvalidateEntry(IdentifierSet identifiers);
    }
}
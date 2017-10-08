using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KenticoCloud.Delivery;
using KenticoCloud.Delivery.InlineContentItems;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient, IDisposable
    {
        #region "Fields"

        private bool _disposed = false;
        protected readonly IMemoryCache _cache;
        protected readonly DeliveryClient _client;

        #endregion

        #region "Properties"

        public int CacheExpirySeconds
        {
            get;
            set;
        }

        public IContentLinkUrlResolver ContentLinkUrlResolver { get => _client.ContentLinkUrlResolver; set => _client.ContentLinkUrlResolver = value; }
        public ICodeFirstModelProvider CodeFirstModelProvider { get => _client.CodeFirstModelProvider; set => _client.CodeFirstModelProvider = value; }
        IInlineContentItemsProcessor IDeliveryClient.InlineContentItemsProcessor => _client.InlineContentItemsProcessor;

        #endregion

        #region "Constructors"

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache)
        {
            if (string.IsNullOrEmpty(projectOptions.Value.DeliveryOptions.ProjectId))
            {
                _client = new DeliveryClient(projectOptions.Value.DeliveryOptions.ProjectId);
            }
            else
            {
                _client = new DeliveryClient(
                    projectOptions.Value.DeliveryOptions
                );
            }

            CacheExpirySeconds = projectOptions.Value.CacheTimeoutSeconds;
            _cache = memoryCache;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Returns a content item as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content item with the specified codename.</returns>
        public async Task<JObject> GetItemJsonAsync(string codename, params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemJsonAsync)}|{codename}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemJsonAsync(codename, parameters));
        }

        /// <summary>
        /// Returns content items as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemsJsonAsync)}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsJsonAsync(parameters));
        }

        /// <summary>
        /// Returns a content item.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemResponse"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse> GetItemAsync(string codename, params IQueryParameter[] parameters)
        {
            return await GetItemAsync(codename, (IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Gets one strongly typed content item by its codename.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemResponse{T}"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, params IQueryParameter[] parameters)
        {
            return await GetItemAsync<T>(codename, (IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns a content item.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">A collection of query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemResponse"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse> GetItemAsync(string codename, IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemAsync)}|{codename}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemAsync(codename, parameters));
        }

        /// <summary>
        /// Gets one strongly typed content item by its codename.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">A collection of query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemResponse{T}"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, IEnumerable<IQueryParameter> parameters = null)
        {
            string cacheKey = $"{nameof(GetItemAsync)}-{typeof(T).FullName}|{codename}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemAsync<T>(codename, parameters));
        }

        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// Returns content items.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse> GetItemsAsync(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns content items.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse> GetItemsAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsAsync(parameters));
        }

        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// Returns strongly typed content items.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse{T}"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync<T>((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// Returns strongly typed content items.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">A collection of query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse{T}"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}-{typeof(T).FullName}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsAsync<T>(parameters));
        }

        /// <summary>
        /// Returns a content type as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content type with the specified codename.</returns>
        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeJsonAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypeJsonAsync(codename));
        }

        /// <summary>
        /// Returns content types as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetTypesJsonAsync)}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypesJsonAsync(parameters));
        }

        /// <summary>
        /// Returns a content type.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The content type with the specified codename.</returns>
        public async Task<ContentType> GetTypeAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypeAsync(codename));
        }

        /// <summary>
        /// Returns content types.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="DeliveryTypeListingResponse"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<DeliveryTypeListingResponse> GetTypesAsync(params IQueryParameter[] parameters)
        {
            return await GetTypesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns content types.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example for paging.</param>
        /// <returns>The <see cref="DeliveryTypeListingResponse"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<DeliveryTypeListingResponse> GetTypesAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetTypesAsync)}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypesAsync(parameters));
        }

        /// <summary>
        /// Returns a content element.
        /// </summary>
        /// <param name="contentTypeCodename">The codename of the content type.</param>
        /// <param name="contentElementCodename">The codename of the content element.</param>
        /// <returns>A content element with the specified codename that is a part of a content type with the specified codename.</returns>
        public async Task<ContentElement> GetContentElementAsync(string contentTypeCodename, string contentElementCodename)
        {
            string cacheKey = $"{nameof(GetContentElementAsync)}|{contentTypeCodename}|{contentElementCodename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetContentElementAsync(contentTypeCodename, contentElementCodename));
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

        #region "Helper methods"

        protected string Join(IEnumerable<string> parameters)
        {
            return parameters != null ? string.Join("|", parameters) : string.Empty;
        }

        protected async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
        {
            var result = _cache.GetOrCreateAsync<T>(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpirySeconds);
                return factory.Invoke();
            });

            return await result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cache.Dispose();
            }

            _disposed = true;
        }

        public Task<JObject> GetTaxonomyJsonAsync(string codename)
        {
            return _client.GetTaxonomyJsonAsync(codename);
        }

        public Task<JObject> GetTaxonomiesJsonAsync(params string[] parameters)
        {
            return _client.GetTaxonomiesJsonAsync(parameters);
        }

        public Task<TaxonomyGroup> GetTaxonomyAsync(string codename)
        {
            return _client.GetTaxonomyAsync(codename);
        }

        public Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(params IQueryParameter[] parameters)
        {
            return _client.GetTaxonomiesAsync(parameters);
        }

        public Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(IEnumerable<IQueryParameter> parameters)
        {
            return _client.GetTaxonomiesAsync(parameters);
        }

        #endregion
    }
}

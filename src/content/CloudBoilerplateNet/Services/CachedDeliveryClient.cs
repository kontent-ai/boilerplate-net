using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KenticoCloud.Delivery;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient, IDisposable
    {
        #region "Fields"

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

        #endregion

        #region "Constructors"

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache)
        {
            if (string.IsNullOrEmpty(projectOptions.Value.KenticoCloudPreviewApiKey))
            {
                _client = new DeliveryClient(projectOptions.Value.KenticoCloudProjectId);
            }
            else
            {
                _client = new DeliveryClient(
                    projectOptions.Value.KenticoCloudProjectId,
                    projectOptions.Value.KenticoCloudPreviewApiKey
                );
            }

            CacheExpirySeconds = projectOptions.Value.CacheTimeoutSeconds;
            _cache = memoryCache;
        }

        #endregion

        #region "Public methods"

        public async Task<JObject> GetItemJsonAsync(string codename, params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemJsonAsync)}|{codename}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemJsonAsync(codename, parameters));
        }

        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemsJsonAsync)}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsJsonAsync(parameters));
        }

        public async Task<DeliveryItemResponse> GetItemAsync(string codename, params IQueryParameter[] parameters)
        {
            return await GetItemAsync(codename, (IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, params IQueryParameter[] parameters)
        {
            return await GetItemAsync<T>(codename, (IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemResponse> GetItemAsync(string codename, IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemAsync)}|{codename}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemAsync(codename, parameters));
        }

        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, IEnumerable<IQueryParameter> parameters = null)
        {
            string cacheKey = $"{nameof(GetItemAsync)}-{typeof(T).FullName}|{codename}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemAsync<T>(codename, parameters));
        }

        public async Task<DeliveryItemListingResponse> GetItemsAsync(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemListingResponse> GetItemsAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsAsync(parameters));
        }

        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync<T>((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}-{typeof(T).FullName}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetItemsAsync<T>(parameters));
        }

        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeJsonAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypeJsonAsync(codename));
        }

        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetTypesJsonAsync)}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypesJsonAsync(parameters));
        }

        public async Task<ContentType> GetTypeAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypeAsync(codename));
        }

        public async Task<DeliveryTypeListingResponse> GetTypesAsync(params IQueryParameter[] parameters)
        {
            return await GetTypesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryTypeListingResponse> GetTypesAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetTypesAsync)}|{Join(parameters?.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetTypesAsync(parameters));
        }

        public async Task<ContentElement> GetContentElementAsync(string contentTypeCodename, string contentElementCodename)
        {
            string cacheKey = $"{nameof(GetContentElementAsync)}|{contentTypeCodename}|{contentElementCodename}";

            return await GetOrCreateAsync(cacheKey, () => _client.GetContentElementAsync(contentTypeCodename, contentElementCodename));
        }

        public void Dispose()
        {
            _cache.Dispose();
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

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KenticoCloud.Delivery;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient
    {
        #region "Fields"

        protected int _cacheTimeoutMinutes;
        protected int _cacheTimeoutSeconds = -1;
        protected readonly IMemoryCache _cache;
        protected readonly DeliveryClient _client;

        #endregion

        #region "Properties"

        protected int CacheExpiryInSeconds
        {
            get
            {
                if (_cacheTimeoutSeconds == -1)
                {
                    _cacheTimeoutSeconds = _cacheTimeoutMinutes * 60;
                }

                return _cacheTimeoutSeconds;
            }
        }

        public IContentLinkUrlResolver ContentLinkUrlResolver { get => _client.ContentLinkUrlResolver; set => _client.ContentLinkUrlResolver = value; }
        public ICodeFirstModelProvider CodeFirstModelProvider { get => _client.CodeFirstModelProvider; set => _client.CodeFirstModelProvider = value; }

        #endregion

        #region "Constructors"

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache, int cacheTimeoutMinutes)
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

            _cacheTimeoutMinutes = cacheTimeoutMinutes;
            _cache = memoryCache;

        }

        #endregion

        #region "Public methods"

        public async Task<JObject> GetItemJsonAsync(string codename, params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemJsonAsync)}|{codename}|{string.Join("|", parameters)}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemJsonAsync(codename, parameters);
            });
        }

        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetItemsJsonAsync)}|{string.Join("|", parameters)}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemsJsonAsync(parameters);
            });
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
            string cacheKey = $"{nameof(GetItemAsync)}|{codename}|{string.Join("|", parameters.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemAsync(codename, parameters);
            });
        }

        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, IEnumerable<IQueryParameter> parameters = null)
        {
            string cacheKey = $"{nameof(GetItemAsync)}-{typeof(T).FullName}|{codename}|{string.Join("|", parameters.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemAsync<T>(codename, parameters);
            });
        }

        public async Task<DeliveryItemListingResponse> GetItemsAsync(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemListingResponse> GetItemsAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}|{string.Join("|", parameters.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemsAsync(parameters);
            });
        }

        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync<T>((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetItemsAsync)}-{typeof(T).FullName}|{string.Join("|", parameters.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetItemsAsync<T>(parameters);
            });
        }

        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeJsonAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetTypeJsonAsync(codename);
            });
        }

        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetTypesJsonAsync)}|{string.Join("|", parameters)}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetTypesJsonAsync(parameters);
            });
        }

        public async Task<ContentType> GetTypeAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetTypeAsync(codename);
            });
        }

        public async Task<DeliveryTypeListingResponse> GetTypesAsync(params IQueryParameter[] parameters)
        {
            return await GetTypesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryTypeListingResponse> GetTypesAsync(IEnumerable<IQueryParameter> parameters)
        {
            string cacheKey = $"{nameof(GetTypesAsync)}|{string.Join("|", parameters.Select(p => p.GetQueryStringParameter()).ToList())}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetTypesAsync(parameters);
            });
        }

        public async Task<ContentElement> GetContentElementAsync(string contentTypeCodename, string contentElementCodename)
        {
            string cacheKey = $"{nameof(GetContentElementAsync)}|{contentTypeCodename}|{contentElementCodename}";

            return await GetOrCreateAsync(cacheKey, () =>
            {
                return _client.GetContentElementAsync(contentTypeCodename, contentElementCodename);
            });
        }

        #endregion

        #region "Helper methods"

        protected async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
        {
            var result = _cache.GetOrCreateAsync<T>(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpiryInSeconds);
                return factory.Invoke();
            });

            return await result;
        }

        #endregion
    }
}

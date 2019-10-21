using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.Boilerplate.Caching.Default
{
    public class CachingDeliveryClient : IDeliveryClient
    {
        protected IDeliveryClient DeliveryClient { get; }
        protected ICacheManager CacheManager { get; }

        public CachingDeliveryClient(ICacheManager cacheManager, IDeliveryClient deliveryClient)
        {
            DeliveryClient = deliveryClient ?? throw new ArgumentNullException(nameof(deliveryClient));
            CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        /// <summary>
        /// Returns a content item as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of linked items.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content item with the specified codename.</returns>
        public async Task<JObject> GetItemJsonAsync(string codename, params string[] parameters)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemJsonKey(codename, parameters),
                () => DeliveryClient.GetItemJsonAsync(codename, parameters),
                response => response != null);
        }

        /// <summary>
        /// Returns content items as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of linked items.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemsJsonKey(parameters),
                () => DeliveryClient.GetItemsJsonAsync(parameters),
                response => response["items"].Any());
        }

        /// <summary>
        /// Returns a content item.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of linked items.</param>
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
        /// <param name="parameters">An array that contains zero or more query parameters, for example for projection or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemResponse{T}"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, params IQueryParameter[] parameters)
        {
            return await GetItemAsync<T>(codename, (IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns a content item.
        /// </summary>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">A collection of query parameters, for example for projection or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemResponse"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse> GetItemAsync(string codename, IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemKey(codename, queryParameters),
                () => DeliveryClient.GetItemAsync(codename, queryParameters),
                response => response != null);
        }

        /// <summary>
        /// Gets one strongly typed content item by its codename.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">A collection of query parameters, for example for projection or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemResponse{T}"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, IEnumerable<IQueryParameter> parameters = null)
        {
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemTypedKey(codename, queryParameters),
                () => DeliveryClient.GetItemAsync<T>(codename, queryParameters),
                response => response != null);
        }

        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// Returns content items.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse> GetItemsAsync(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns content items.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example for filtering, ordering or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse> GetItemsAsync(IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemsKey(queryParameters),
                () => DeliveryClient.GetItemsAsync(queryParameters),
                response => response.Items.Any());
        }

        /// <summary>
        /// Searches the content repository for items that match the filter criteria.
        /// Returns strongly typed content items.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse{T}"/> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(params IQueryParameter[] parameters)
        {
            return await GetItemsAsync<T>((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns strongly typed content items that match the optional filtering parameters. By default, retrieves one level of linked items.
        /// </summary>
        /// <typeparam name="T">Type of the model. (Or <see cref="object" /> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">A collection of query parameters, for example, for filtering, ordering, or setting the depth of linked items.</param>
        /// <returns>The <see cref="DeliveryItemListingResponse{T}" /> instance that contains the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetItemsTypedKey(queryParameters),
                () => DeliveryClient.GetItemsAsync<T>(queryParameters),
                response => response.Items.Any());
        }

        /// <summary>
        /// Returns a feed that is used to traverse through content items matching the optional filtering parameters.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example, for filtering or ordering.</param>
        /// <returns>The <see cref="IDeliveryItemsFeed" /> instance that can be used to enumerate through content items. If no query parameters are specified, all content items are enumerated.</returns>
        public IDeliveryItemsFeed GetItemsFeed(params IQueryParameter[] parameters)
        {
            return GetItemsFeed((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns a feed that is used to traverse through content items matching the optional filtering parameters.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example, for filtering or ordering.</param>
        /// <returns>The <see cref="IDeliveryItemsFeed" /> instance that can be used to enumerate through content items. If no query parameters are specified, all content items are enumerated.</returns>
        public IDeliveryItemsFeed GetItemsFeed(IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters as IQueryParameter[] ?? parameters?.ToArray() ?? Array.Empty<IQueryParameter>();
            return new CachingDeliveryItemsFeed(queryParameters, DeliveryClient.GetItemsFeed(queryParameters), CacheManager);
        }

        /// <summary>
        /// Returns a feed that is used to traverse through strongly typed content items matching the optional filtering parameters.
        /// </summary>
        /// <typeparam name="T">Type of the model. (Or <see cref="object" /> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">A collection of query parameters, for example, for filtering or ordering.</param>
        /// <returns>The <see cref="IDeliveryItemsFeed{T}" /> instance that can be used to enumerate through content items. If no query parameters are specified, all content items are enumerated.</returns>
        public IDeliveryItemsFeed<T> GetItemsFeed<T>(params IQueryParameter[] parameters)
        {
            return GetItemsFeed<T>((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns a feed that is used to traverse through strongly typed content items matching the optional filtering parameters.
        /// </summary>
        /// <typeparam name="T">Type of the model. (Or <see cref="object" /> if the return type is not yet known.)</typeparam>
        /// <param name="parameters">A collection of query parameters, for example, for filtering or ordering.</param>
        /// <returns>The <see cref="IDeliveryItemsFeed{T}" /> instance that can be used to enumerate through content items. If no query parameters are specified, all content items are enumerated.</returns>
        public IDeliveryItemsFeed<T> GetItemsFeed<T>(IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters as IQueryParameter[] ?? parameters?.ToArray() ?? Array.Empty<IQueryParameter>();
            return new CachingDeliveryItemsFeed<T>(queryParameters, DeliveryClient.GetItemsFeed<T>(queryParameters), CacheManager);
        }

        /// <summary>
        /// Returns a content type as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content type with the specified codename.</returns>
        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTypeJsonKey(codename),
                () => DeliveryClient.GetTypeJsonAsync(codename),
                response => response != null);
        }

        /// <summary>
        /// Returns content types as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTypesJsonKey(parameters),
                () => DeliveryClient.GetTypesJsonAsync(parameters),
                response => response["types"].Any());
        }
        
        /// <summary>
        /// Returns a content type.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The content type with the specified codename.</returns>
        public async Task<DeliveryTypeResponse> GetTypeAsync(string codename)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTypeKey(codename),
                () => DeliveryClient.GetTypeAsync(codename),
                response => response != null);
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
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTypesKey(queryParameters),
                () => DeliveryClient.GetTypesAsync(queryParameters),
                response => response.Types.Any());
        }

        /// <summary>
        /// Returns a content element.
        /// </summary>
        /// <param name="contentTypeCodename">The codename of the content type.</param>
        /// <param name="contentElementCodename">The codename of the content element.</param>
        /// <returns>A content element with the specified codename that is a part of a content type with the specified codename.</returns>
        public async Task<DeliveryElementResponse> GetContentElementAsync(string contentTypeCodename, string contentElementCodename)
        {

            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetContentElementKey(contentTypeCodename, contentElementCodename),
                () => DeliveryClient.GetContentElementAsync(contentTypeCodename, contentElementCodename),
                response => response != null);
        }

        /// <summary>
        /// Returns a taxonomy group as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a taxonomy group.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the taxonomy group with the specified codename.</returns>
        public async Task<JObject> GetTaxonomyJsonAsync(string codename)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTaxonomyJsonKey(codename),
                () => DeliveryClient.GetTaxonomyJsonAsync(codename),
                response => response != null);
        }

        /// <summary>
        /// Returns taxonomy groups as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example, for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the taxonomy groups. If no query parameters are specified, all taxonomy groups are returned.</returns>
        public async Task<JObject> GetTaxonomiesJsonAsync(params string[] parameters)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTaxonomiesJsonKey(parameters),
                () => DeliveryClient.GetTaxonomiesJsonAsync(parameters),
                response => response["taxonomies"].Any());
        }
        
        /// <summary>
        /// Returns a taxonomy group.
        /// </summary>
        /// <param name="codename">The codename of a taxonomy group.</param>
        /// <returns>The taxonomy group with the specified codename.</returns>
        public async Task<DeliveryTaxonomyResponse> GetTaxonomyAsync(string codename)
        {
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTaxonomyKey(codename),
                () => DeliveryClient.GetTaxonomyAsync(codename),
                response => response != null);
        }

        /// <summary>
        /// Returns taxonomy groups.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example, for paging.</param>
        /// <returns>The <see cref="DeliveryTaxonomyListingResponse"/> instance that represents the taxonomy groups. If no query parameters are specified, all taxonomy groups are returned.</returns>
        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(params IQueryParameter[] parameters)
        {
            return await GetTaxonomiesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        /// <summary>
        /// Returns taxonomy groups.
        /// </summary>
        /// <param name="parameters">A collection of query parameters, for example, for paging.</param>
        /// <returns>The <see cref="DeliveryTaxonomyListingResponse"/> instance that represents the taxonomy groups. If no query parameters are specified, all taxonomy groups are returned.</returns>
        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(IEnumerable<IQueryParameter> parameters)
        {
            var queryParameters = parameters?.ToList();
            return await CacheManager.GetOrAddAsync(
                CacheHelper.GetTaxonomiesKey(queryParameters),
                () => DeliveryClient.GetTaxonomiesAsync(queryParameters),
                response => response.Taxonomies.Any());
        }

        public class CachingDeliveryItemsFeed : IDeliveryItemsFeed
        {
            private readonly IEnumerable<IQueryParameter> _parameters;
            private readonly IDeliveryItemsFeed _itemsFeed;
            private readonly ICacheManager _cacheManager;
            private string _continuationToken;

            public bool HasMoreResults { get; private set; } = true;

            public CachingDeliveryItemsFeed(IEnumerable<IQueryParameter> parameters, IDeliveryItemsFeed itemsFeed, ICacheManager cacheManager)
            {
                _parameters = parameters;
                _itemsFeed = itemsFeed;
                _cacheManager = cacheManager;
            }

            public async Task<DeliveryItemsFeedResponse> FetchNextBatchAsync()
            {
                var response = await _cacheManager.GetOrAddAsync(
                    CacheHelper.GetItemsFeedKey(_parameters, _continuationToken),
                    () => _itemsFeed.FetchNextBatchAsync(),
                    r => r != null);

                _continuationToken = response?.ContinuationToken;
                HasMoreResults = !string.IsNullOrEmpty(_continuationToken);
                return response;
            }
        }

        public class CachingDeliveryItemsFeed<T> : IDeliveryItemsFeed<T>
        {
            private readonly IEnumerable<IQueryParameter> _parameters;
            private readonly IDeliveryItemsFeed<T> _itemsFeed;
            private readonly ICacheManager _cacheManager;
            private string _continuationToken;

            public bool HasMoreResults { get; private set; } = true;

            public CachingDeliveryItemsFeed(IEnumerable<IQueryParameter> parameters, IDeliveryItemsFeed<T> itemsFeed, ICacheManager cacheManager)
            {
                _parameters = parameters;
                _itemsFeed = itemsFeed;
                _cacheManager = cacheManager;
            }

            public async Task<DeliveryItemsFeedResponse<T>> FetchNextBatchAsync()
            {
                var response = await _cacheManager.GetOrAddAsync(
                    CacheHelper.GetItemsFeedTypedKey(_parameters, _continuationToken),
                    () => _itemsFeed.FetchNextBatchAsync(),
                    r => r != null);

                _continuationToken = response?.ContinuationToken;
                HasMoreResults = !string.IsNullOrEmpty(_continuationToken);
                return response;
            }
        }
    }
}

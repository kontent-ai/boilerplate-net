using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using CloudBoilerplateNet.Extensions;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;
using KenticoCloud.Delivery.InlineContentItems;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient
    {
        #region "Properties"

        protected DeliveryClient DeliveryClient { get; }
        protected ICacheManager CacheManager { get; }
        protected ProjectOptions ProjectOptions { get; }

        public IContentLinkUrlResolver ContentLinkUrlResolver
        {
            get => DeliveryClient.ContentLinkUrlResolver;
            set => DeliveryClient.ContentLinkUrlResolver = value;
        }

        public ICodeFirstModelProvider CodeFirstModelProvider
        {
            get => DeliveryClient.CodeFirstModelProvider;
            set => DeliveryClient.CodeFirstModelProvider = value;
        }

        public HttpClient HttpClient
        {
            get => DeliveryClient.HttpClient;
            set => DeliveryClient.HttpClient = value;
        }

        public IInlineContentItemsProcessor InlineContentItemsProcessor => DeliveryClient.InlineContentItemsProcessor;

        #endregion

        #region "Constructors"

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, ICacheManager cacheManager)
        {
            ProjectOptions = projectOptions.Value;
            DeliveryClient = new DeliveryClient(ProjectOptions.DeliveryOptions);
            CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
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
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER, codename };
            identifierTokens.AddNonNullRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemJsonAsync(codename, parameters), GetContentItemSingleJsonDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns content items as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddNonNullRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsJsonAsync(parameters), GetContentItemListingJsonDependencies, ProjectOptions.CreateCacheEntriesInBackground);
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
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, codename };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemAsync(codename, parameters), GetContentItemSingleDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Gets one strongly typed content item by its codename.
        /// </summary>
        /// <typeparam name="T">Type of the code-first model. (Or <see cref="object"/> if the return type is not yet known.)</typeparam>
        /// <param name="codename">The codename of a content item.</param>
        /// <param name="parameters">A collection of query parameters, for example for projection or depth of modular content.</param>
        /// <returns>The <see cref="DeliveryItemResponse{T}"/> instance that contains the content item with the specified codename.</returns>
        public async Task<DeliveryItemResponse<T>> GetItemAsync<T>(string codename, IEnumerable<IQueryParameter> parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER, codename };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemAsync<T>(codename, parameters), GetContentItemSingleDependencies, ProjectOptions.CreateCacheEntriesInBackground);
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
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_IDENTIFIER };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsAsync(parameters), GetContentItemListingDependencies, ProjectOptions.CreateCacheEntriesInBackground);
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

        public async Task<DeliveryItemListingResponse<T>> GetItemsAsync<T>(IEnumerable<IQueryParameter> parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_TYPED_IDENTIFIER };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsAsync<T>(parameters), GetContentItemListingDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns a content type as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content type with the specified codename.</returns>
        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypeJsonAsync(codename), GetTypeSingleJsonDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns content types as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddNonNullRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypesJsonAsync(parameters), GetTypeListingJsonDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns a content type.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The content type with the specified codename.</returns>
        public async Task<ContentType> GetTypeAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypeAsync(codename), GetTypeSingleDependencies, ProjectOptions.CreateCacheEntriesInBackground);
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
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_LISTING_IDENTIFIER };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypesAsync(parameters), GetTypeListingDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns a content element.
        /// </summary>
        /// <param name="contentTypeCodename">The codename of the content type.</param>
        /// <param name="contentElementCodename">The codename of the content element.</param>
        /// <returns>A content element with the specified codename that is a part of a content type with the specified codename.</returns>
        public async Task<ContentElement> GetContentElementAsync(string contentTypeCodename, string contentElementCodename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, contentTypeCodename, contentElementCodename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetContentElementAsync(contentTypeCodename, contentElementCodename), GetContentElementDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns a taxonomy group as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a taxonomy group.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the taxonomy group with the specified codename.</returns>
        public async Task<JObject> GetTaxonomyJsonAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomyJsonAsync(codename), GetTaxonomySingleJsonDependency, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns taxonomy groups as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example, for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the taxonomy groups. If no query parameters are specified, all taxonomy groups are returned.</returns>
        public async Task<JObject> GetTaxonomiesJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddNonNullRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomiesJsonAsync(parameters), GetTaxonomyListingJsonDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        /// <summary>
        /// Returns a taxonomy group.
        /// </summary>
        /// <param name="codename">The codename of a taxonomy group.</param>
        /// <returns>The taxonomy group with the specified codename.</returns>
        public async Task<TaxonomyGroup> GetTaxonomyAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomyAsync(codename), GetTaxonomySingleDependency, ProjectOptions.CreateCacheEntriesInBackground);
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
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_IDENTIFIER };
            identifierTokens.AddNonNullRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomiesAsync(parameters), GetTaxonomyListingDependencies, ProjectOptions.CreateCacheEntriesInBackground);
        }

        #region "Dependency resolvers"

        /// <summary>
        /// Extracts identifier sets from a single-item response.
        /// </summary>
        /// <param name="response">The <see cref="DeliveryItemResponse"/> or <see cref="DeliveryItemResponse{T}"/>, either strongly-typed, or runtime-typed.</param>
        /// <returns>Identifiers of all formats of the item, its modular content items, taxonomies used in elements, underlying content type and eventually its elements (when present in the cache).</returns>
        public IEnumerable<IdentifierSet> GetContentItemSingleDependencies(dynamic response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemSingleResponse(response) && response?.Item != null)
            {
                dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(response.Item));

                foreach (var item in response.ModularContent)
                {
                    dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(item));
                }
            }

            return dependencies.Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a single-item JSON response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> response.</param>
        /// <returns>Identifiers of all formats of the item, its modular content items, taxonomies used in elements, underlying content type and eventually its elements (when present in the cache).</returns>
        public IEnumerable<IdentifierSet> GetContentItemSingleJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemSingleJsonResponse(response))
            {
                dependencies.AddNonNullRange(GetContentItemDependencies(response[KenticoCloudCacheHelper.ITEM_IDENTIFIER]));

                foreach (var item in response[KenticoCloudCacheHelper.MODULAR_CONTENT_IDENTIFIER]?.Children())
                {
                    dependencies.AddNonNullRange(GetContentItemDependencies(item));
                }
            }

            return dependencies.Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from an item listing response.
        /// </summary>
        /// <param name="response">The <see cref="DeliveryItemListingResponse"/> or <see cref="DeliveryItemListingResponse{T}"/>, either strongly-typed, or runtime-typed.</param>
        /// <returns>Identifiers of all formats of the items, their modular content items, taxonomies used in elements, underlying content types and eventually their elements (when present in the cache).</returns>
        public IEnumerable<IdentifierSet> GetContentItemListingDependencies(dynamic response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemListingResponse(response))
            {
                foreach (dynamic item in response.Items)
                {
                    dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(item));
                }

                foreach (var item in response.ModularContent)
                {
                    dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(item));
                }
            }

            return dependencies.Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a item listing JSON response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> item listing response.</param>
        /// <returns>Identifiers of all formats of the items, their modular content items, taxonomies used in elements, underlying content types and eventually their elements (when present in the cache).</returns>
        public IEnumerable<IdentifierSet> GetContentItemListingJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemListingJsonResponse(response))
            {
                foreach (dynamic item in response[KenticoCloudCacheHelper.ITEMS_IDENTIFIER].Children())
                {
                    dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(item));
                }

                foreach (var item in response[KenticoCloudCacheHelper.MODULAR_CONTENT_IDENTIFIER]?.Children())
                {
                    dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentItemDependencies(item));
                }
            }

            return dependencies.Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a content element response.
        /// </summary>
        /// <param name="response">The <see cref="ContentElement"/> response.</param>
        /// <returns>Identifiers of all formats of the element.</returns>
        public IEnumerable<IdentifierSet> GetContentElementDependencies(ContentElement response)
        {
            return GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, response).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a singnle taxonomy group response.
        /// </summary>
        /// <param name="response">The <see cref="TaxonomyGroup"/> response.</param>
        /// <returns>Identifiers of all formats of the taxonomy.</returns>
        public IEnumerable<IdentifierSet> GetTaxonomySingleDependency(TaxonomyGroup response)
        {
            return GetTaxonomyDependencies(KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, response?.System?.Codename).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a single taxonomy group JSON response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> taxonomy response.</param>
        /// <returns>Identifiers of all formats of the taxonomy.</returns>
        public IEnumerable<IdentifierSet> GetTaxonomySingleJsonDependency(JObject response)
        {
            return GetTaxonomyDependencies(
                KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER,
                response?[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString()).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a listing taxonomy group response.
        /// </summary>
        /// <param name="response">The <see cref="DeliveryTaxonomyListingResponse"/> response.</param>
        /// <returns>Identifiers of all formats of all the taxonomies.</returns>
        public IEnumerable<IdentifierSet> GetTaxonomyListingDependencies(DeliveryTaxonomyListingResponse response)
        {
            return response?.Taxonomies?.SelectMany(t => GetTaxonomySingleDependency(t)).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a listing taxonomy group JSON response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> response.</param>
        /// <returns>Identifiers of all formats of all the taxonomies.</returns>
        public IEnumerable<IdentifierSet> GetTaxonomyListingJsonDependencies(JObject response)
        {
            return response?[KenticoCloudCacheHelper.TAXONOMIES_IDENTIFIER]?.SelectMany(t => GetTaxonomySingleJsonDependency(t.ToObject<JObject>())).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a content type response.
        /// </summary>
        /// <param name="response">The <see cref="ContentType"/> response.</param>
        /// <returns>Identifiers of all formats of the content type and its elements.</returns>
        public IEnumerable<IdentifierSet> GetTypeSingleDependencies(ContentType response)
        {
            return GetContentTypeDependencies(KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, response?.System?.Codename, response).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a content type response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> content type response.</param>
        /// <returns>Identifiers of all formats of the content type and its elements.</returns>
        public IEnumerable<IdentifierSet> GetTypeSingleJsonDependencies(JObject response)
        {
            return GetContentTypeDependencies(
                KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER,
                response?[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString(), response).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a content type listing response.
        /// </summary>
        /// <param name="response">The <see cref="DeliveryTypeListingResponse"/> response.</param>
        /// <returns>Identifiers of all formats of all the content types and their elements.</returns>
        public IEnumerable<IdentifierSet> GetTypeListingDependencies(DeliveryTypeListingResponse response)
        {
            return response?.Types?.SelectMany(t => GetTypeSingleDependencies(t)).Distinct();
        }

        /// <summary>
        /// Extracts identifier sets from a content type listing JSON response.
        /// </summary>
        /// <param name="response">The <see cref="JObject"/> content type listing response.</param>
        /// <returns>Identifiers of all formats of all the content types and their elements.</returns>
        public IEnumerable<IdentifierSet> GetTypeListingJsonDependencies(JObject response)
        {
            return response?[KenticoCloudCacheHelper.TYPES_IDENTIFIER]?.SelectMany(t => GetTypeSingleJsonDependencies(t.ToObject<JObject>())).Distinct();
        }

        #endregion

        #endregion

        #region "Protected methods"

        protected IEnumerable<IdentifierSet> GetContentItemDependencies(dynamic item)
        {
            var dependencies = new List<IdentifierSet>();
            string extractedItemCodename, extractedTypeCodename;
            KenticoCloudCacheHelper.ExtractCodenamesFromItem(item, out extractedItemCodename, out extractedTypeCodename);

            if (!string.IsNullOrEmpty(extractedItemCodename))
            {
                // Dependency on all formats of the item.
                dependencies.AddNonNullRange(
                    CacheManager.GetDependenciesByType(KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, extractedItemCodename, identifierSet =>
                        Enumerable.Repeat(identifierSet, 1)
                    ));

                // Dependency on elements of item's type (if possible).
                dependencies.AddNonNullRange(GetContentTypeDependencies(KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, extractedTypeCodename));

                // Dependency on item's taxonomy elements.
                foreach (string taxonomyElementCodename in KenticoCloudCacheHelper.GetItemTaxonomyCodenamesByElements(item))
                {
                    dependencies.AddNonNullRange(
                        GetTaxonomyDependencies(KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, taxonomyElementCodename)
                    );
                }
            }

            return dependencies;
        }

        protected IEnumerable<IdentifierSet> GetTaxonomyDependencies(string originalFormatIdentifier, string taxonomyCodename)
        {
            return CacheManager.GetDependenciesByType(
                originalFormatIdentifier, taxonomyCodename, identifierSet =>
                    Enumerable.Repeat(identifierSet, 1)
            );
        }

        protected IEnumerable<IdentifierSet> GetContentTypeDependencies(string originalFormatIdentifier, string contentTypeCodeName, dynamic response = null)
        {
            var dependencies = new List<IdentifierSet>();

            dependencies.AddNonNullRange(
                CacheManager.GetDependenciesByType(originalFormatIdentifier, contentTypeCodeName, identifierSet =>
                    Enumerable.Repeat(identifierSet, 1)
                ));

            // Try to get element codenames from the response.
            if (response != null)
            {
                if (response is ContentType && response?.Elements != null)
                {
                    foreach (var element in response.Elements)
                    {
                        dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, element.Value));
                    }
                }
                else if (response is JObject)
                {
                    var elements = response?[KenticoCloudCacheHelper.ELEMENTS_IDENTIFIER];

                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            dependencies.AddNonNullRange((IEnumerable<IdentifierSet>)GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_JSON_IDENTIFIER, element));
                        }
                    }
                }
            }
            // If no response exists, try to get element codenames from the cache.
            else
            {
                dependencies.AddNonNullRange(
                    CacheManager.GetDependenciesByType(
                        originalFormatIdentifier, contentTypeCodeName, identifierSet =>
                        {
                            return CacheManager.GetDependenciesByName<ContentType>(identifierSet, cachedContentType =>
                            {
                                var dependenciesPerCacheEntry = new List<IdentifierSet>();

                                foreach (var elementDependency in cachedContentType?.Elements?.SelectMany(e => GetContentElementDependencies(e.Value)))
                                {
                                    dependenciesPerCacheEntry.Add(elementDependency);
                                }

                                if (!string.IsNullOrEmpty(identifierSet.Type))
                                {
                                    dependenciesPerCacheEntry.Add(new IdentifierSet
                                    {
                                        Type = identifierSet.Type,
                                        Codename = cachedContentType.System?.Codename
                                    });
                                }

                                return dependenciesPerCacheEntry;
                            });
                        }
                ));
            }

            return dependencies;
        }

        protected IEnumerable<IdentifierSet> GetContentElementDependenciesInternal(string originalFormatIdentifier, dynamic response)
        {
            var dependencies = new List<IdentifierSet>();
            string elementCodename = null;
            string elementType = null;

            if (response is ContentElement)
            {
                elementCodename = response?.Codename?.ToString();
                elementType = response?.Type?.ToString();
            }
            else if (response is JProperty)
            {
                elementCodename = response?.Name?.ToString();
                elementType = response?.Value?[KenticoCloudCacheHelper.TYPE_IDENTIFIER]?.ToString();
            }

            if (!string.IsNullOrEmpty(elementType) && !string.IsNullOrEmpty(elementCodename))
            {
                dependencies.AddNonNullRange(
                    CacheManager.GetDependenciesByType(
                        originalFormatIdentifier, elementCodename, identifierSet =>
                            new List<IdentifierSet>
                            {
                                new IdentifierSet
                                {
                                    Type = identifierSet.Type,
                                    Codename = string.Join("|", elementType, elementCodename)
                                }
                            }
                ));
            }

            return dependencies;
        }

        #endregion
    }
}

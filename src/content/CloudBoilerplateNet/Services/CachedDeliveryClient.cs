using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using KenticoCloud.Delivery;
using KenticoCloud.Delivery.InlineContentItems;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using System.Net.Http;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient
    {
        #region "Properties"

        protected DeliveryClient DeliveryClient { get; }
        protected ICacheManager CacheManager { get; }
        protected IDependentTypesResolver DependentTypesResolver { get; }

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

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, ICacheManager cacheManager, IDependentTypesResolver dependentTypesResolver)
        {
            DeliveryClient = new DeliveryClient(projectOptions.Value.DeliveryOptions);
            CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            DependentTypesResolver = dependentTypesResolver ?? throw new ArgumentNullException(nameof(dependentTypesResolver));
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
            identifierTokens.AddRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemJsonAsync(codename, parameters), GetContentItemSingleJsonDependencies);
        }

        /// <summary>
        /// Returns content items as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for filtering, ordering or depth of modular content.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content items. If no query parameters are specified, all content items are returned.</returns>
        public async Task<JObject> GetItemsJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsJsonAsync(parameters), GetContentItemListingJsonDependencies);
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
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemAsync(codename, parameters), GetContentItemSingleDependencies);
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
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemAsync<T>(codename, parameters), GetContentItemSingleDependencies);
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
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsAsync(parameters), GetContentItemListingDependencies);
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
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsAsync<T>(parameters), GetContentItemListingDependencies);
        }

        /// <summary>
        /// Returns a content type as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content type with the specified codename.</returns>
        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypeJsonAsync(codename), GetTypeSingleJsonDependencies);
        }

        /// <summary>
        /// Returns content types as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypesJsonAsync(parameters), GetTypeListingJsonDependencies);
        }

        /// <summary>
        /// Returns a content type.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The content type with the specified codename.</returns>
        public async Task<ContentType> GetTypeAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypeAsync(codename), GetTypeSingleDependencies);
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
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTypesAsync(parameters), GetTypeListingDependencies);
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

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetContentElementAsync(contentTypeCodename, contentElementCodename), GetContentElementDependencies);
        }

        public async Task<JObject> GetTaxonomyJsonAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomyJsonAsync(codename), GetTaxonomySingleJsonDependency);
        }

        public async Task<JObject> GetTaxonomiesJsonAsync(params string[] parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER };
            identifierTokens.AddRange(parameters);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomiesJsonAsync(parameters), GetTaxonomyListingJsonDependencies);
        }

        public async Task<TaxonomyGroup> GetTaxonomyAsync(string codename)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, codename };

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomyAsync(codename), GetTaxonomySingleDependency);
        }

        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(params IQueryParameter[] parameters)
        {
            return await GetTaxonomiesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(IEnumerable<IQueryParameter> parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_IDENTIFIER };
            identifierTokens.AddRange(KenticoCloudCacheHelper.GetIdentifiersFromParameters(parameters));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomiesAsync(parameters), GetTaxonomyListingDependencies);
        }

        #region "Dependency resolvers"

        public IEnumerable<IdentifierSet> GetContentItemSingleDependencies(dynamic response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemSingleResponse(response) && response?.Item != null)
            {
                dependencies.AddRange(GetContentItemDependencies(response.Item));

                foreach (var item in response.ModularContent)
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }
            }

            return dependencies.Distinct();
        }

        public IEnumerable<IdentifierSet> GetContentItemSingleJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemSingleJsonResponse(response))
            {
                dependencies.AddRange(GetContentItemDependencies(response[KenticoCloudCacheHelper.ITEM_IDENTIFIER]));

                foreach (var item in response[KenticoCloudCacheHelper.MODULAR_CONTENT_IDENTIFIER]?.Children())
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }
            }

            //dependencies.AddRange(KenticoCloudCacheHelper.GetJsonModularContentDependencies(response));

            //var ownDependency = new IdentifierSet
            //{
            //    Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
            //    Codename = KenticoCloudCacheHelper.GetContentItemSingleCodenameFromJson(response)
            //};

            //if (response?[KenticoCloudCacheHelper.ITEM_IDENTIFIER] != null)
            //{
            //    dependencies.AddRange(KenticoCloudCacheHelper.GetContentItemJsonTaxonomyDependencies(response[KenticoCloudCacheHelper.ITEM_IDENTIFIER]));
            //}

            //dependencies.AddRange(KenticoCloudCacheHelper.GetJsonModularContentDependencies(response));

            //if (!dependencies.Contains(ownDependency))
            //{
            //    dependencies.Add(ownDependency);
            //}

            //dependencies.AddRange(GetContentItemJsonTypeDependencies(response?[KenticoCloudCacheHelper.ITEM_IDENTIFIER]));

            return dependencies.Distinct();
        }

        public IEnumerable<IdentifierSet> GetContentItemListingDependencies(dynamic response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemListingResponse(response))
            {
                foreach (dynamic item in response.Items)
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }

                foreach (var item in response.ModularContent)
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }
            }

            //dependencies.AddRange(KenticoCloudCacheHelper.GetModularContentDependencies(response));

            return dependencies.Distinct();
        }

        public IEnumerable<IdentifierSet> GetContentItemListingJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();

            if (KenticoCloudCacheHelper.IsDeliveryItemListingJsonResponse(response))
            {
                foreach (dynamic item in response[KenticoCloudCacheHelper.ITEMS_IDENTIFIER].Children())
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }

                foreach (var item in response[KenticoCloudCacheHelper.MODULAR_CONTENT_IDENTIFIER]?.Children())
                {
                    dependencies.AddRange(GetContentItemDependencies(item));
                }
            }

            //foreach (var mc in KenticoCloudCacheHelper.GetJsonModularContentDependencies(response))
            //{
            //    yield return mc;
            //}

            //foreach (var item in response[KenticoCloudCacheHelper.ITEMS_IDENTIFIER].Children())
            //{
            //    yield return new IdentifierSet
            //    {
            //        Type = KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER,
            //        Codename = item[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER].ToString()
            //    };

            //    foreach (var typeDependency in GetContentItemJsonTypeDependencies(item))
            //    {
            //        yield return typeDependency;
            //    }
            //}

            return dependencies.Distinct();
        }

        //public IEnumerable<IdentifierSet> GetContentElementDependency(ContentElement response)
        //{
        //    //return GetContentElementDependencyInternal(response, KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER);
        //    return (response != null) ? new List<IdentifierSet>
        //    {
        //        new IdentifierSet
        //        {
        //            Type = KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER,
        //            Codename = string.Join("|", response.Type, response.Codename)
        //        }
        //    } : null;
        //}

        //protected IEnumerable<IdentifierSet> GetContentElementDependencyInternal(ContentElement contentElement, string typeIdentifier)
        //{
        //}

        public IEnumerable<IdentifierSet> GetContentElementDependencies(ContentElement response)
        {
            return GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, response).Distinct();
        }


        public IEnumerable<IdentifierSet> GetTaxonomySingleDependency(TaxonomyGroup response)
        {
            return GetTaxonomyDependencies(KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, response?.System?.Codename).Distinct();
            //return response != null ? new List<IdentifierSet>
            //{
            //    new IdentifierSet
            //    {
            //        Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER,
            //        Codename = response.System?.Codename
            //    }
            //} : null;
        }

        public IEnumerable<IdentifierSet> GetTaxonomySingleJsonDependency(JObject response)
        {
            return GetTaxonomyDependencies(
                KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER,
                response?[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString()).Distinct();
            //return response != null ? new List<IdentifierSet>
            //{
            //    new IdentifierSet
            //    {
            //        Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER,
            //        Codename = response[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString()
            //    }
            //} : null;
        }

        public IEnumerable<IdentifierSet> GetTaxonomyListingDependencies(DeliveryTaxonomyListingResponse response)
        {
            return response?.Taxonomies?.SelectMany(t => GetTaxonomySingleDependency(t)).Distinct();
        }

        public IEnumerable<IdentifierSet> GetTaxonomyListingJsonDependencies(JObject response)
        {
            return response?[KenticoCloudCacheHelper.TAXONOMIES_IDENTIFIER]?.SelectMany(t => GetTaxonomySingleJsonDependency(t.ToObject<JObject>())).Distinct();
        }

        public IEnumerable<IdentifierSet> GetTypeSingleDependencies(ContentType response)
        {
            return GetContentTypeDependencies(KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, response?.System?.Codename, response).Distinct();
            //return GetTypeSingleDependenciesInternal(response, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER);
        }

        //protected IEnumerable<IdentifierSet> GetTypeSingleDependenciesInternal(ContentType contentType, string typeIdentifier)
        //{
        //    foreach (var element in contentType?.Elements.SelectMany(e => GetContentElementDependencies(e.Value)))
        //    {
        //        yield return element;
        //    }

        //    if (!string.IsNullOrEmpty(typeIdentifier))
        //    {
        //        yield return new IdentifierSet
        //        {
        //            Type = typeIdentifier,
        //            Codename = contentType.System?.Codename
        //        };
        //    }
        //}

        public IEnumerable<IdentifierSet> GetTypeSingleJsonDependencies(JObject response)
        {
            return GetContentTypeDependencies(
                KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER,
                response?[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString(), response).Distinct();
            //foreach (var element in response?[KenticoCloudCacheHelper.ELEMENTS_IDENTIFIER])
            //{
            //    if (!string.IsNullOrEmpty((element as JProperty)?.Name))
            //    {
            //        yield return new IdentifierSet
            //        {
            //            Type = KenticoCloudCacheHelper.CONTENT_ELEMENT_JSON_IDENTIFIER,
            //            Codename = (element as JProperty)?.Name
            //        };
            //    }
            //}

            //yield return new IdentifierSet
            //{
            //    Type = KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER,
            //    Codename = response[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString()
            //};
        }

        public IEnumerable<IdentifierSet> GetTypeListingDependencies(DeliveryTypeListingResponse response)
        {
            return response?.Types?.SelectMany(t => GetTypeSingleDependencies(t)).Distinct();
        }

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
                //// Dependency on the item itself.
                //dependencies.Add(new IdentifierSet
                //{
                //    Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                //    Codename = extractedItemCodename
                //});

                // Dependency on all formats of the item.
                dependencies.AddRange(
                    GetDependenciesForAllDependentFormats(KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, extractedItemCodename, (itemCodename, formatIdentifier) =>
                        new List<IdentifierSet>
                        {
                            new IdentifierSet
                            {
                                Type = formatIdentifier,
                                Codename = itemCodename
                            }
                        }
                    ));

                // Dependency on elements of item's type.
                //dependencies.AddRange(
                //    GetDependenciesForAllDependentFormats(
                //        extractedTypeCodename, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, (typeCodename, formatIdentifier) =>
                //        {
                //            return GetDependenciesByCacheContents<ContentType>(formatIdentifier, typeCodename, (cachedContentType, identifier) =>
                //                GetTypeSingleDependenciesInternal(cachedContentType, identifier));
                //        }
                //));
                dependencies.AddRange(GetContentTypeDependencies(KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, extractedTypeCodename));

                // Dependency on item's taxonomy elements.
                foreach (string taxonomyElementCodename in KenticoCloudCacheHelper.GetItemTaxonomyCodenamesByElements(item))
                {
                    dependencies.AddRange(
                        GetTaxonomyDependencies(KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, taxonomyElementCodename)
                    //GetDependenciesForAllDependentFormats(
                    //    taxonomyElementCodename, KenticoCloudCacheHelper.TAXONOMY_GROUP_IDENTIFIER, (taxonomyCodename, formatIdentifier) =>
                    //    {
                    //        return GetDependenciesByCacheContents<TaxonomyGroup>(formatIdentifier, taxonomyCodename, (cachedTaxonomyGroup, identifier) =>
                    //            new List<IdentifierSet> { new IdentifierSet { Type = identifier, Codename = cachedTaxonomyGroup.System.Codename } });
                    //    }
                    //)
                    );
                }
            }

            return dependencies;
        }

        protected IEnumerable<IdentifierSet> GetTaxonomyDependencies(string originalFormatIdentifier, string taxonomyCodename)
        {
            return GetDependenciesForAllDependentFormats(
                originalFormatIdentifier, taxonomyCodename, (codename, formatIdentifier) =>
                    new List<IdentifierSet> { new IdentifierSet { Type = formatIdentifier, Codename = codename } }
            //{
            //    return GetDependenciesByCacheContents<TaxonomyGroup>(formatIdentifier, codename, (cachedTaxonomyGroup, identifier) =>
            //        new List<IdentifierSet> { new IdentifierSet { Type = identifier, Codename = cachedTaxonomyGroup.System.Codename } });
            //}
            );
        }

        protected IEnumerable<IdentifierSet> GetContentTypeDependencies(string originalFormatIdentifier, string contentTypeCodeName, dynamic response = null)
        {
            var dependencies = new List<IdentifierSet>();

            dependencies.AddRange(
                GetDependenciesForAllDependentFormats(originalFormatIdentifier, contentTypeCodeName, (typeCodename, formatIdentifier) =>
                    new List<IdentifierSet>
                    {
                        new IdentifierSet
                        {
                            Type = formatIdentifier,
                            Codename = typeCodename
                        }
                    }
                ));

            //dependencies.Add(new IdentifierSet { Type = originalFormatIdentifier, Codename = contentTypeCodeName });

            // Try to get element codenames from the response.
            if (response != null)
            {
                if (response is ContentType && response?.Elements != null)
                {

                    //response.Elements.Select(e => dependencies.AddRange(GetContentElementDependencies(e.Value)));
                    foreach (var element in response.Elements)
                    {
                        dependencies.AddRange(GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, element.Value));
                    }
                }
                else if (response is JObject)
                {
                    var elements = response?[KenticoCloudCacheHelper.ELEMENTS_IDENTIFIER];

                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            dependencies.AddRange(GetContentElementDependenciesInternal(KenticoCloudCacheHelper.CONTENT_ELEMENT_JSON_IDENTIFIER, element));
                        }
                    }
                }
            }
            // If no response exists, try to get element codenames from the cache.
            else
            {
                dependencies.AddRange(
                    GetDependenciesForAllDependentFormats(
                        originalFormatIdentifier, contentTypeCodeName, (typeCodename, formatIdentifier) =>
                        {
                            return GetDependenciesByCacheContents<ContentType>(formatIdentifier, typeCodename, (cachedContentType, identifier) =>
                            {
                                var dependenciesPerCacheEntry = new List<IdentifierSet>();

                                foreach (var element in cachedContentType?.Elements.SelectMany(e => GetContentElementDependencies(e.Value)))
                                {
                                    dependenciesPerCacheEntry.Add(element);
                                }

                                if (!string.IsNullOrEmpty(formatIdentifier))
                                {
                                    dependenciesPerCacheEntry.Add(new IdentifierSet
                                    {
                                        Type = formatIdentifier,
                                        Codename = cachedContentType.System?.Codename
                                    });
                                }

                                return dependenciesPerCacheEntry;
                            });
                            //GetTypeSingleDependenciesInternal(cachedContentType, identifier));
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
                dependencies.AddRange(
                    GetDependenciesForAllDependentFormats(
                        originalFormatIdentifier, elementCodename, (codename, formatIdentifier) =>
                            new List<IdentifierSet>
                            {
                                new IdentifierSet
                                {
                                    Type = formatIdentifier,
                                    Codename = string.Join("|", elementType, elementCodename)
                                }
                            }
                ));
            }

            return dependencies;
        }

        protected IEnumerable<IdentifierSet> GetDependenciesByCacheContents<T>(string formatIdentifier, string codename, Func<T, string, IEnumerable<IdentifierSet>> dependencyFactory)
            where T : class
        {
            var dependencies = new List<IdentifierSet>();

            if (CacheManager.TryGetValue(new[] { formatIdentifier, codename }, out T cacheEntry))
            {
                return dependencyFactory(cacheEntry, formatIdentifier);
            }

            return dependencies;
        }

        protected IEnumerable<IdentifierSet> GetDependenciesForAllDependentFormats(string originalFormatIdentifier, string codename, Func<string, string, IEnumerable<IdentifierSet>> dependencyFactory)
        {
            var dependencies = new List<IdentifierSet>();

            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(originalFormatIdentifier) && dependencyFactory != null)
            {
                foreach (var formatIdentifier in DependentTypesResolver.GetDependentTypeNames(originalFormatIdentifier))
                {
                    dependencies.AddRange(dependencyFactory(codename, formatIdentifier));
                }
            }

            return dependencies;
        }

        //protected IEnumerable<IdentifierSet> GetContentItemTypeDependencies(dynamic item)
        //{
        //    string contentTypeCodename = item?.System?.Type?.ToString();

        //    return GetContentItemTypeDependenciesInternal(contentTypeCodename, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER);
        //}

        //private IEnumerable<IdentifierSet> GetContentItemTypeDependenciesInternal(string contentTypeCodename, string originalFormatIdentifier)
        //{
        //    if (!string.IsNullOrEmpty(contentTypeCodename) && !string.IsNullOrEmpty(originalFormatIdentifier))
        //    {
        //        foreach (var formatIdentifier in DependentTypesResolver.GetDependentTypeNames(originalFormatIdentifier))
        //        {
        //            if (CacheManager.TryGetValue(new[] { KenticoCloudCacheHelper.DUMMY_IDENTIFIER, formatIdentifier, contentTypeCodename }, out ContentType contentType))
        //            {
        //                return GetTypeSingleDependenciesInternal(contentType, formatIdentifier);
        //            }
        //        }
        //    }

        //    return new List<IdentifierSet>();
        //}

        //protected IEnumerable<IdentifierSet> GetContentItemJsonFormatDependencies(JToken itemToken)
        //{
        //    var itemCodename = itemToken[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.CODENAME_IDENTIFIER]?.ToString();

        //    if (!string.IsNullOrEmpty(itemCodename))
        //    {
        //        foreach (var formatIdentifier in DependentTypesResolver.GetDependentTypeNames(KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER))
        //        {
        //            yield return new IdentifierSet
        //            {
        //                Type = formatIdentifier,
        //                Codename = itemCodename
        //            };
        //        }
        //    }
        //}

        //protected IEnumerable<IdentifierSet> GetContentItemJsonTypeDependencies(JToken itemToken)
        //{
        //    var contentTypeCodename = itemToken?[KenticoCloudCacheHelper.SYSTEM_IDENTIFIER][KenticoCloudCacheHelper.TYPE_IDENTIFIER]?.ToString();

        //    return GetContentItemTypeDependenciesInternal(contentTypeCodename, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER);
        //}

        #endregion
    }
}

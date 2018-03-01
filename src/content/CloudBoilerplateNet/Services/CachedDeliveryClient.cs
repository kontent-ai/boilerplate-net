using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using KenticoCloud.Delivery;
using KenticoCloud.Delivery.InlineContentItems;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public class CachedDeliveryClient : IDeliveryClient, IDisposable
    {
        #region "Fields"

        private bool _disposed = false;

        #endregion

        #region "Properties"

        protected IMemoryCache Cache { get; }
        protected ICacheManager CacheManager { get; }
        protected DeliveryClient DeliveryClient { get; }

        public int CacheExpirySeconds
        {
            get;
            set;
        }

        public IContentLinkUrlResolver ContentLinkUrlResolver { get => DeliveryClient.ContentLinkUrlResolver; set => DeliveryClient.ContentLinkUrlResolver = value; }
        public ICodeFirstModelProvider CodeFirstModelProvider { get => DeliveryClient.CodeFirstModelProvider; set => DeliveryClient.CodeFirstModelProvider = value; }
        public IInlineContentItemsProcessor InlineContentItemsProcessor => DeliveryClient.InlineContentItemsProcessor;

        #endregion

        #region "Constructors"

        public CachedDeliveryClient(IOptions<ProjectOptions> projectOptions, ICacheManager cacheManager, IMemoryCache memoryCache)
        {
            DeliveryClient = new DeliveryClient(projectOptions.Value.DeliveryOptions);
            CacheExpirySeconds = projectOptions.Value.CacheTimeoutSeconds;
            CacheManager = cacheManager;
            Cache = memoryCache; // TODO remove
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
            AddIdentifiersFromParameters(parameters, identifierTokens);

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
            AddIdentifiersFromParameters(parameters, identifierTokens);

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
            AddIdentifiersFromParameters(parameters, identifierTokens);

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
            AddIdentifiersFromParameters(parameters, identifierTokens);

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetItemsAsync<T>(parameters), GetContentItemListingDependencies);
        }

        /// <summary>
        /// Returns a content type as JSON data.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content type with the specified codename.</returns>
        public async Task<JObject> GetTypeJsonAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeJsonAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTypeJsonAsync(codename));
        }

        /// <summary>
        /// Returns content types as JSON data.
        /// </summary>
        /// <param name="parameters">An array that contains zero or more query parameters, for example for paging.</param>
        /// <returns>The <see cref="JObject"/> instance that represents the content types. If no query parameters are specified, all content types are returned.</returns>
        public async Task<JObject> GetTypesJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetTypesJsonAsync)}|{Join(parameters)}";

            return await GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTypesJsonAsync(parameters));
        }

        /// <summary>
        /// Returns a content type.
        /// </summary>
        /// <param name="codename">The codename of a content type.</param>
        /// <returns>The content type with the specified codename.</returns>
        public async Task<ContentType> GetTypeAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTypeAsync)}|{codename}";

            return await GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTypeAsync(codename));
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

            return await GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTypesAsync(parameters));
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

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetContentElementAsync(contentTypeCodename, contentElementCodename), GetContentElementDependency);
        }

        public Task<JObject> GetTaxonomyJsonAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTaxonomyJsonAsync)}|{codename}";
            return GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTaxonomyJsonAsync(codename));
        }

        public Task<JObject> GetTaxonomiesJsonAsync(params string[] parameters)
        {
            string cacheKey = $"{nameof(GetTaxonomiesJsonAsync)}|{Join(parameters)}";
            return GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTaxonomiesJsonAsync(parameters));
        }

        public Task<TaxonomyGroup> GetTaxonomyAsync(string codename)
        {
            string cacheKey = $"{nameof(GetTaxonomyAsync)}|{codename}";
            return GetOrCreateAsync(cacheKey, () => DeliveryClient.GetTaxonomyAsync(codename));
        }

        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(params IQueryParameter[] parameters)
        {
            return await GetTaxonomiesAsync((IEnumerable<IQueryParameter>)parameters);
        }

        public async Task<DeliveryTaxonomyListingResponse> GetTaxonomiesAsync(IEnumerable<IQueryParameter> parameters)
        {
            var identifierTokens = new List<string> { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_IDENTIFIER };
            identifierTokens.AddRange(parameters.Select(p => p.GetQueryStringParameter()));

            return await CacheManager.GetOrCreateAsync(identifierTokens, () => DeliveryClient.GetTaxonomiesAsync(parameters), GetTaxonomyListingDependencies);
        }

        #region "Dependency resolvers"

        public static IEnumerable<IdentifierSet> GetContentItemSingleDependencies<T>(T response)
        {
            var dependencies = new List<IdentifierSet>();
            AddModularContentDependencies(response, dependencies);

            // Create dummy item for the content item itself.
            var ownDependency = new IdentifierSet
            {
                Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                Codename = GetContentItemCodename(response)
            };

            if (!dependencies.Contains(ownDependency))
            {
                dependencies.Add(ownDependency);
            }

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetContentItemListingDependencies<T>(T response)
        {
            var dependencies = new List<IdentifierSet>();
            AddModularContentDependencies(response, dependencies);

            // Create dummy item for each content item in the listing.
            foreach (var codename in GetContentItemCodenamesFromListingResponse(response))
            {
                var dependency = new IdentifierSet
                {
                    Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                    Codename = codename
                };

                if (!dependencies.Contains(dependency))
                {
                    dependencies.Add(dependency);
                }
            }

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetContentItemSingleJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();
            AddJsonModularContentDependencies(response, dependencies);

            // Create dummy item for the content item itself.
            var ownDependency = new IdentifierSet
            {
                Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                Codename = GetContentItemSingleCodenameFromJson(response)
            };

            if (!dependencies.Contains(ownDependency))
            {
                dependencies.Add(ownDependency);
            }

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetContentItemListingJsonDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();
            AddJsonModularContentDependencies(response, dependencies);

            foreach (var item in response["items"].Children())
            {
                dependencies.Add(new IdentifierSet
                {
                    Type = KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER,

                    // TODO Cast or ToString?
                    Codename = item["system"].Value<string>("codename").ToString()
                });
            }

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetContentElementDependency<T>(T response)
        {
            var dependencies = new List<IdentifierSet>();

            if (response is ContentElement && response != null)
            {
                dependencies.Add(
                    new IdentifierSet
                    {
                        Type = KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER,
                        Codename = GetContentElementCodename(response)
                    });
            }

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetTaxonomyListingDependencies<T>(T response)
        {
            var dependencies = new List<IdentifierSet>();

            if (response is DeliveryTaxonomyListingResponse && response != null)
            {
                foreach (var taxonomyGroupCodename in GetTaxonomyListingCodenames(response))
                {
                    dependencies.Add(
                        new IdentifierSet
                        {
                            Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER,
                            Codename = taxonomyGroupCodename
                        });
                }
            }

            return dependencies;
        }

        #endregion

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
            var result = Cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpirySeconds);
                return factory.Invoke();
            });

            return await result;
        }

        private static string GetContentItemCodename(dynamic response)
        {
            if (response.Item?.System?.Codename != null)
            {
                try
                {
                    return response.Item.System.Codename;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetContentItemCodenamesFromListingResponse(dynamic response)
        {
            if (response?.Items != null)
            {
                var codenames = new List<string>();

                foreach (dynamic item in response.Items)
                {
                    try
                    {
                        codenames.Add(item.System?.Codename);
                    }
                    catch
                    {
                        return null;
                    }
                }

                return codenames;
            }

            return null;
        }

        private static void AddModularContentDependencies<T>(T response, List<IdentifierSet> dependencies)
        {
            foreach (var codename in GetModularContentCodenames(response))
            {
                dependencies.Add(new IdentifierSet
                {
                    Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                    Codename = codename
                });
            }
        }

        private static IEnumerable<string> GetModularContentCodenames(dynamic response)
        {
            if (response.ModularContent != null)
            {
                foreach (var mc in response.ModularContent)
                {
                    yield return mc.Path;
                }
            }
        }

        private static void AddIdentifiersFromParameters(IEnumerable<IQueryParameter> parameters, List<string> identifierTokens)
        {
            if (parameters != null)
            {
                identifierTokens.AddRange(parameters?.Select(p => p.GetQueryStringParameter()));
            }
        }

        private static void AddJsonModularContentDependencies(JObject response, List<IdentifierSet> dependencies)
        {
            const string MODULAR_CONTENT_IDENTIFIER = "modular_content";

            if (response[MODULAR_CONTENT_IDENTIFIER].HasValues)
            {
                foreach (var mcToken in response[MODULAR_CONTENT_IDENTIFIER])
                {
                    // TODO SelectMany?
                    foreach (var item in mcToken)
                    {
                        dependencies.Add(new IdentifierSet
                        {
                            Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,

                            // TODO Cast or ToString?
                            Codename = item["system"].Value<string>("codename").ToString()
                        });
                    }
                }
            }
        }

        private static string GetContentItemSingleCodenameFromJson(JObject response)
        {
            return response["item"]["system"]["codename"].ToString();
        }

        private static string GetContentElementCodename(dynamic response)
        {
            if (response is ContentElement && response != null)
            {
                return string.Join("|", response.Type, response.Codename);
            }

            return null;
        }

        private static IEnumerable<string> GetTaxonomyListingCodenames(dynamic response)
        {
            if (response.Taxonomies != null)
            {
                foreach (dynamic taxonomyGroup in response.Taxonomies)
                {
                    yield return taxonomyGroup.System?.Codename;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Cache.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}

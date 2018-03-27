using System;
using System.Collections.Generic;
using System.Linq;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Newtonsoft.Json.Linq;

namespace CloudBoilerplateNet.Helpers
{
    public class KenticoCloudCacheHelper : IDependentTypesResolver
    {
        protected const string LISTING_SUFFIX = "_listing";
        protected const string JSON_SUFFIX = "_json";
        protected const string TYPED_SUFFIX = "_typed";
        protected const string RUNTIME_TYPED_SUFFIX = "_runtime_typed";

        public const string CONTENT_ITEM_SINGLE_IDENTIFIER = "content_item";
        public const string CONTENT_ITEM_SINGLE_JSON_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + TYPED_SUFFIX;
        public const string CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + RUNTIME_TYPED_SUFFIX;
        public const string CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + "_variant";
        public const string CONTENT_ITEM_LISTING_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string CONTENT_ITEM_LISTING_JSON_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ITEM_LISTING_TYPED_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + TYPED_SUFFIX;
        public const string CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + RUNTIME_TYPED_SUFFIX;
        public const string CONTENT_TYPE_SINGLE_IDENTIFIER = "content_type";
        public const string CONTENT_TYPE_JSON_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_TYPE_LISTING_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string CONTENT_TYPE_LISTING_JSON_IDENTIFIER = CONTENT_TYPE_LISTING_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ELEMENT_IDENTIFIER = "content_element";
        public const string CONTENT_ELEMENT_JSON_IDENTIFIER = CONTENT_ELEMENT_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_SINGLE_IDENTIFIER = "taxonomy";
        public const string TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER = TAXONOMY_GROUP_LISTING_IDENTIFIER + JSON_SUFFIX;

        public const string CODENAME_IDENTIFIER = "codename";
        public const string SYSTEM_IDENTIFIER = "system";
        public const string MODULAR_CONTENT_IDENTIFIER = "modular_content";
        public const string ITEM_IDENTIFIER = "item";
        public const string ITEMS_IDENTIFIER = "items";
        public const string TYPE_IDENTIFIER = "type";
        public const string TYPES_IDENTIFIER = "types";
        public const string TAXONOMIES_IDENTIFIER = "taxonomies";
        public const string ELEMENTS_IDENTIFIER = "elements";
        private const string TAXONOMY_GROUP_IDENTIFIER = "taxonomy_group";

        public static IEnumerable<string> ContentItemSingleRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_ITEM_SINGLE_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER,
                    CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER,
                };
            }
        }

        public static IEnumerable<string> ContentItemListingRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_ITEM_LISTING_IDENTIFIER,
                    CONTENT_ITEM_LISTING_JSON_IDENTIFIER,
                    CONTENT_ITEM_LISTING_TYPED_IDENTIFIER,
                    CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentTypeSingleRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_TYPE_SINGLE_IDENTIFIER,
                    CONTENT_TYPE_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentTypeListingRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_TYPE_LISTING_IDENTIFIER,
                    CONTENT_TYPE_LISTING_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentElementRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_ELEMENT_IDENTIFIER,
                    CONTENT_ELEMENT_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> TaxonomyGroupSingleRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    TAXONOMY_GROUP_SINGLE_IDENTIFIER,
                    TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> TaxonomyGroupListingRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    TAXONOMY_GROUP_LISTING_IDENTIFIER,
                    TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER
                };
            }
        }

        public IEnumerable<string> GetDependentTypes(string typeCodeName)
        {
            if (ContentItemSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return ContentItemSingleRelatedTypes;
            }
            else if (ContentItemListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return ContentItemListingRelatedTypes;
            }
            else if (ContentTypeSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return ContentTypeSingleRelatedTypes;
            }
            else if (ContentTypeListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return ContentTypeListingRelatedTypes;
            }
            else if (typeCodeName.Equals(CONTENT_ELEMENT_IDENTIFIER, StringComparison.Ordinal))
            {
                return ContentElementRelatedTypes;
            }
            else if (TaxonomyGroupSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return TaxonomyGroupSingleRelatedTypes;
            }
            else if (TaxonomyGroupListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return TaxonomyGroupListingRelatedTypes;
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<string> GetContentItemCodenamesFromListingResponse(dynamic response)
        {
            if (IsDeliveryListingResponse(response))
            {
                foreach (dynamic item in response.Items)
                {
                    if (!string.IsNullOrEmpty(item.System?.Codename))
                    {
                        yield return item.System?.Codename;
                    }
                }
            }
        }

        public static IEnumerable<IdentifierSet> GetModularContentDependencies(dynamic response)
        {
            if (IsDeliveryResponse(response))
            {
                var dependencies = new List<IdentifierSet>();

                foreach (var item in (response.ModularContent as JObject))
                {
                    dependencies.Add(new IdentifierSet
                    {
                        Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                        Codename = item.Value[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER].ToString()
                    });

                    IEnumerable<IdentifierSet> taxonomyDependencies = GetContentItemJsonTaxonomyDependencies(item.Value.ToObject<JObject>());
                    dependencies.AddRange(taxonomyDependencies.Where(i => !dependencies.Contains(i)));
                }

                return dependencies;
            }

            return null;
        }

        // TODO: Unit tests
        public static IEnumerable<IdentifierSet> GetJsonModularContentDependencies(JObject response)
        {
            var dependencies = new List<IdentifierSet>();

            foreach (var item in response?[MODULAR_CONTENT_IDENTIFIER])
            {
                dependencies.AddRange(item.Children()[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.Select(codename =>
                {
                    return new IdentifierSet
                    {
                        Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                        Codename = codename.ToString()
                    };
                }));

                IEnumerable<IdentifierSet> taxonomyDependencies = item.Children().SelectMany(child => GetContentItemJsonTaxonomyDependencies(child.ToObject<JObject>()));
                dependencies.AddRange(taxonomyDependencies.Where(i => !dependencies.Contains(i)));
            };

            return dependencies;
        }

        public static IEnumerable<IdentifierSet> GetContentItemJsonTaxonomyDependencies(JObject responseFragment)
        {
            var taxonomyElements = responseFragment?[ELEMENTS_IDENTIFIER]?.SelectMany(t => t.Children())?
                                .Where(e => e[TYPE_IDENTIFIER] != null && e[TYPE_IDENTIFIER].ToString().Equals(TAXONOMY_GROUP_SINGLE_IDENTIFIER, StringComparison.Ordinal) && e[TAXONOMY_GROUP_IDENTIFIER] != null && !string.IsNullOrEmpty(e[TAXONOMY_GROUP_IDENTIFIER].ToString()));

            return taxonomyElements.Select(e => new IdentifierSet
            {
                Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER,
                Codename = e[TAXONOMY_GROUP_IDENTIFIER].ToString()
            });
        }

        // TODO: Unit tests
        public static IEnumerable<string> GetModularContentCodenames(dynamic response)
        {
            // if (response.ModularContent != null && response.ModularContent is System.Collections.IEnumerable) is not completely safe
            if (IsDeliveryResponse(response))
            {
                foreach (var mc in response.ModularContent)
                {
                    if (!string.IsNullOrEmpty(mc.Path))
                    {
                        yield return mc.Path;
                    }
                }
            }
        }

        public static bool IsDeliveryResponse(dynamic response)
        {
            if (IsDeliverySingleResponse(response) || IsDeliveryListingResponse(response))
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliverySingleResponse(dynamic response)
        {
            if (response is DeliveryItemResponse || (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemResponse<>)))
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliveryListingResponse(dynamic response)
        {
            if (response is DeliveryItemListingResponse || (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemListingResponse<>)))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<string> GetIdentifiersFromParameters(IEnumerable<IQueryParameter> parameters)
        {
            return parameters?.Select(p => p.GetQueryStringParameter());
        }

        public static string GetContentItemSingleCodenameFromJson(JObject response)
        {
            return response?[ITEM_IDENTIFIER][SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.ToString();
        }
    }
}

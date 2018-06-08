using System;
using System.Collections.Generic;
using System.Linq;

using KenticoCloud.Delivery;
using Newtonsoft.Json.Linq;

namespace CloudBoilerplateNet.Helpers
{
    public static class KenticoCloudCacheHelper
    {
        #region "Constants"

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
        public const string CONTENT_TYPE_SINGLE_JSON_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_TYPE_LISTING_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string CONTENT_TYPE_LISTING_JSON_IDENTIFIER = CONTENT_TYPE_LISTING_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ELEMENT_IDENTIFIER = "content_element";
        public const string CONTENT_ELEMENT_JSON_IDENTIFIER = CONTENT_ELEMENT_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_SINGLE_IDENTIFIER = "taxonomy";
        public const string TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER = TAXONOMY_GROUP_LISTING_IDENTIFIER + JSON_SUFFIX;

        public const string DUMMY_IDENTIFIER = "dummy";
        public const string CODENAME_IDENTIFIER = "codename";
        public const string SYSTEM_IDENTIFIER = "system";
        public const string MODULAR_CONTENT_IDENTIFIER = "modular_content";
        public const string ITEM_IDENTIFIER = "item";
        public const string ITEMS_IDENTIFIER = "items";
        public const string TYPE_IDENTIFIER = "type";
        public const string TYPES_IDENTIFIER = "types";
        public const string TAXONOMIES_IDENTIFIER = "taxonomies";
        public const string ELEMENTS_IDENTIFIER = "elements";
        public const string TAXONOMY_GROUP_IDENTIFIER = "taxonomy_group";

        private const string LISTING_SUFFIX = "_listing";
        private const string JSON_SUFFIX = "_json";
        private const string TYPED_SUFFIX = "_typed";
        private const string RUNTIME_TYPED_SUFFIX = "_runtime_typed";

        #endregion

        #region "Properties"

        public static IEnumerable<string> ContentItemSingleRelatedFormats
        {
            get
            {
                return new[]
                {
                    CONTENT_ITEM_SINGLE_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER,
                    CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER,
                };
            }
        }

        public static IEnumerable<string> ContentItemListingRelatedFormats
        {
            get
            {
                return new[]
                {
                    CONTENT_ITEM_LISTING_IDENTIFIER,
                    CONTENT_ITEM_LISTING_JSON_IDENTIFIER,
                    CONTENT_ITEM_LISTING_TYPED_IDENTIFIER,
                    CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentTypeSingleRelatedFormats
        {
            get
            {
                return new[]
                {
                    CONTENT_TYPE_SINGLE_IDENTIFIER,
                    CONTENT_TYPE_SINGLE_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentTypeListingRelatedFormats
        {
            get
            {
                return new[]
                {
                    CONTENT_TYPE_LISTING_IDENTIFIER,
                    CONTENT_TYPE_LISTING_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> ContentElementRelatedFormats
        {
            get
            {
                return new[]
                {
                    CONTENT_ELEMENT_IDENTIFIER,
                    CONTENT_ELEMENT_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> TaxonomyGroupSingleRelatedFormats
        {
            get
            {
                return new[]
                {
                    TAXONOMY_GROUP_SINGLE_IDENTIFIER,
                    TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER
                };
            }
        }

        public static IEnumerable<string> TaxonomyGroupListingRelatedFormats
        {
            get
            {
                return new[]
                {
                    TAXONOMY_GROUP_LISTING_IDENTIFIER,
                    TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER
                };
            }
        }

        #endregion

        #region "Public methods"

        public static IEnumerable<string> GetItemJsonTaxonomyCodenamesByElements(JToken elementsToken)
        {
            var taxonomyElements = elementsToken?.SelectMany(t => t.Children())?.Where(
                        e => e[TYPE_IDENTIFIER] != null && e[TYPE_IDENTIFIER].ToString().Equals(TAXONOMY_GROUP_SINGLE_IDENTIFIER, StringComparison.Ordinal) &&
                        e[TAXONOMY_GROUP_IDENTIFIER] != null && !string.IsNullOrEmpty(e[TAXONOMY_GROUP_IDENTIFIER].ToString()));

            return taxonomyElements.Select(e => e[TAXONOMY_GROUP_IDENTIFIER].ToString());
        }

        public static IEnumerable<string> GetItemTaxonomyCodenamesByElements(dynamic item)
        {
            if (item is ContentItem)
            {
                return GetItemJsonTaxonomyCodenamesByElements(item?.Elements);
            }
            else if (item is JObject)
            {
                return GetItemJsonTaxonomyCodenamesByElements(item?[ELEMENTS_IDENTIFIER]);
            }
            else
            {
                var codenames = new List<string>();
                var properties = item?.GetType().GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType.GenericTypeArguments.Length > 0 && 
                        property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) && 
                        property.PropertyType.GenericTypeArguments[0] == typeof(TaxonomyTerm))
                    {
                        var codenameProperty = item.GetType().GetField($"{property.Name}Codename");
                        codenames.Add(codenameProperty.GetValue(item) as string);
                    }
                }
                return codenames;
            }
        }

        public static bool IsDeliveryItemSingleResponse(dynamic response)
        {
            return (response is DeliveryItemResponse || 
                (response.GetType().IsGenericType && 
                response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemResponse<>))) ? true : false;
        }

        public static bool IsDeliveryItemListingResponse(dynamic response)
        {
            return (response is DeliveryItemListingResponse || 
                (response.GetType().IsGenericType && 
                response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemListingResponse<>))) ? true : false;
        }

        public static bool IsDeliveryItemSingleJsonResponse(JObject response)
        {
            return (response?[ITEM_IDENTIFIER] != null) ? true : false;
        }

        public static bool IsDeliveryItemListingJsonResponse(JObject response)
        {
            return (response?[ITEMS_IDENTIFIER] != null) ? true : false;
        }

        public static IEnumerable<string> GetIdentifiersFromParameters(IEnumerable<IQueryParameter> parameters)
        {
            return parameters?.Select(p => p.GetQueryStringParameter());
        }

        public static void ExtractCodenamesFromItem(dynamic item, out string extractedItemCodename, out string extractedTypeCodename)
        {
            extractedItemCodename = null;
            extractedTypeCodename = null;

            if ((item is ContentItem || !(item is JProperty) && !(item is JObject)) && item?.System != null)
            {
                extractedItemCodename = item.System.Codename?.ToString();
                extractedTypeCodename = item.System.Type?.ToString();
            }
            else if (item is JProperty && item?.Value?[SYSTEM_IDENTIFIER] != null)
            {
                extractedItemCodename = item.Value[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.ToString();
                extractedTypeCodename = item.Value[SYSTEM_IDENTIFIER][TYPE_IDENTIFIER]?.ToString();
            }
            else if (item is JObject && item[SYSTEM_IDENTIFIER] != null)
            {
                extractedItemCodename = item?[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.ToString();
                extractedTypeCodename = item?[SYSTEM_IDENTIFIER][TYPE_IDENTIFIER]?.ToString();
            }
        }

        #endregion
    }
}

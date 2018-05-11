using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Newtonsoft.Json.Linq;

namespace CloudBoilerplateNet.Helpers
{
    public static class KenticoCloudCacheHelper
    {
        private const string LISTING_SUFFIX = "_listing";
        private const string JSON_SUFFIX = "_json";
        private const string TYPED_SUFFIX = "_typed";
        private const string RUNTIME_TYPED_SUFFIX = "_runtime_typed";

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

        //public static IEnumerable<string> GetContentItemCodenamesFromListingResponse(dynamic response)
        //{
        //    if (IsDeliveryItemListingResponse(response))
        //    {
        //        foreach (dynamic item in response.Items)
        //        {
        //            if (!string.IsNullOrEmpty(item.System?.Codename))
        //            {
        //                yield return item.System?.Codename;
        //            }
        //        }
        //    }
        //}

        //public static IEnumerable<IdentifierSet> GetModularContentDependencies(dynamic response)
        //{
        //    if (IsDeliveryResponse(response))
        //    {
        //        var dependencies = new List<IdentifierSet>();

        //        foreach (var item in (response.ModularContent as JObject))
        //        {
        //            dependencies.Add(new IdentifierSet
        //            {
        //                Type = CONTENT_ITEM_SINGLE_IDENTIFIER,
        //                Codename = item.Value[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER].ToString()
        //            });

        //            IEnumerable<IdentifierSet> taxonomyDependencies = GetContentItemJsonTaxonomyDependencies(item.Value);
        //            dependencies.AddRange(taxonomyDependencies.Where(i => !dependencies.Contains(i)));
        //        }

        //        return dependencies;
        //    }

        //    return null;
        //}

        //public static IEnumerable<IdentifierSet> GetJsonModularContentDependencies(JObject response)
        //{
        //    var dependencies = new List<IdentifierSet>();

        //    foreach (var item in response?[MODULAR_CONTENT_IDENTIFIER])
        //    {
        //        dependencies.AddRange(item.Children()[SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.Select(codename =>
        //        {
        //            return new IdentifierSet
        //            {
        //                Type = CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
        //                Codename = codename.ToString()
        //            };
        //        }));

        //        IEnumerable<IdentifierSet> taxonomyDependencies = item.Children().SelectMany(child => GetContentItemJsonTaxonomyDependencies(child));
        //        dependencies.AddRange(taxonomyDependencies.Where(i => !dependencies.Contains(i)));
        //    };

        //    return dependencies;
        //}

        //public static IEnumerable<IdentifierSet> GetContentItemTaxonomyDependencies(dynamic contentItem)
        //{
        //    if (contentItem?.Elements != null) //TODO Does not work with strongly-typed items!
        //    {
        //        return GetContentItemJsonTaxonomyDependencies(contentItem.Elements);
        //    }

        //    return new List<IdentifierSet>();
        //}

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

                //var properties = elementsToken?.GetType().GetTypeInfo().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType.GenericTypeArguments.Length > 0 && property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) && property.PropertyType.GenericTypeArguments[0] == typeof(TaxonomyTerm))
                    {
                        var codenameProperty = item.GetType().GetField($"{property.Name}Codename");
                        codenames.Add(codenameProperty.GetValue(item) as string);
                    }
                }
                //return new List<string>();
                return codenames;
            }
        }

        //public static IEnumerable<IdentifierSet> GetContentItemJsonTaxonomyDependencies(JToken itemToken)
        //{
        //    var taxonomyElements = itemToken?[ELEMENTS_IDENTIFIER]?.SelectMany(t => t.Children())?.Where(
        //                            e => e[TYPE_IDENTIFIER] != null && e[TYPE_IDENTIFIER].ToString().Equals(TAXONOMY_GROUP_SINGLE_IDENTIFIER, StringComparison.Ordinal) && 
        //                            e[TAXONOMY_GROUP_IDENTIFIER] != null && !string.IsNullOrEmpty(e[TAXONOMY_GROUP_IDENTIFIER].ToString()));

        //    return taxonomyElements.Select(e => new IdentifierSet
        //    {
        //        Type = TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER, //TODO Does not work with non-JSON origins
        //        Codename = e[TAXONOMY_GROUP_IDENTIFIER].ToString()
        //    });
        //}

        //public static IEnumerable<string> GetModularContentCodenames(dynamic response)
        //{
        //    if (IsDeliveryResponse(response))
        //    {
        //        foreach (var mc in response.ModularContent)
        //        {
        //            if (!string.IsNullOrEmpty(mc.Path))
        //            {
        //                yield return mc.Path;
        //            }
        //        }
        //    }
        //}

        public static bool IsDeliveryResponse(dynamic response)
        {
            if (IsDeliveryItemSingleResponse(response) || IsDeliveryItemListingResponse(response))
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliveryItemSingleResponse(dynamic response)
        {
            if (response is DeliveryItemResponse || (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemResponse<>)))
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliveryItemListingResponse(dynamic response)
        {
            if (response is DeliveryItemListingResponse || (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(DeliveryItemListingResponse<>)))
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliveryItemSingleJsonResponse(JObject response)
        {
            if (response?[ITEM_IDENTIFIER] != null)
            {
                return true;
            }

            return false;
        }

        public static bool IsDeliveryItemListingJsonResponse(JObject response)
        {
            if (response?[ITEMS_IDENTIFIER] != null)
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<string> GetIdentifiersFromParameters(IEnumerable<IQueryParameter> parameters)
        {
            return parameters?.Select(p => p.GetQueryStringParameter());
        }

        //public static string GetContentItemSingleCodenameFromJson(JObject response)
        //{
        //    return response?[ITEM_IDENTIFIER][SYSTEM_IDENTIFIER][CODENAME_IDENTIFIER]?.ToString();
        //}

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
    }
}

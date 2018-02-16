using System;
using System.Collections.Generic;
using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;

namespace CloudBoilerplateNet.Helpers
{
    public static class CacheHelper
    {
        private const string LISTING_SUFFIX = "_listing";
        private const string JSON_SUFFIX = "_json";
        private const string TYPED_SUFFIX = "_typed";
        private const string RUNTIME_TYPED_SUFFIX = "_runtime_typed";

        public const string CONTENT_ITEM_SINGLE_IDENTIFIER = "content_item";
        public const string CONTENT_ITEM_SINGLE_JSON_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ITEM_LISTING_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string CONTENT_ITEM_LISTING_JSON_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + "_variant";
        public const string CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + TYPED_SUFFIX;
        public const string CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER = CONTENT_ITEM_SINGLE_IDENTIFIER + RUNTIME_TYPED_SUFFIX;
        public const string CONTENT_ITEM_LISTING_TYPED_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + TYPED_SUFFIX;
        public const string CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER = CONTENT_ITEM_LISTING_IDENTIFIER + RUNTIME_TYPED_SUFFIX;
        public const string CONTENT_ELEMENT_IDENTIFIER = "content_element";
        public const string TAXONOMY_GROUP_IDENTIFIER = "taxonomy";
        public const string TAXONOMY_GROUP_JSON_IDENTIFIER = TAXONOMY_GROUP_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_IDENTIFIER = TAXONOMY_GROUP_IDENTIFIER + LISTING_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER = TAXONOMY_GROUP_LISTING_IDENTIFIER + JSON_SUFFIX;

        //private static readonly Dictionary<Type, string> typeIdentifiers = new Dictionary<Type, string>
        //{
        //    { typeof(DeliveryItemResponse), "content_item" },
        //    { typeof(DeliveryItemResponse<>), "content_item_typed" }
        //};

        //public static Dictionary<Type, string> TypeIdentifiers => typeIdentifiers;

        public static IEnumerable<string> GetDependentTypes(IdentifierSet identifierSet)
        {
            switch (identifierSet.Type)
            {
                
                default:
                    break;
            }
        }

        private static IEnumerable<string> GetSingleContentItemRelatedTypes(string)
        {
            new List<string>
            {
                CONTENT_ITEM_SINGLE_IDENTIFIER,
                CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER
            };
        }

        private static IEnumerable<string> GetSingleContentItemDependentTypes(IdentifierSet identifierSet)
        {
            var ownDependentTypes = new List<string>
            {
                CONTENT_ITEM_SINGLE_IDENTIFIER,
                CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER
            };

            ownDependentTypes.AddRange()
        }

        public static IEnumerable<string> GetTaxonomyGroupListingDependentTypes(IdentifierSet identifierSet)
        {

        }
    }

    //public enum Format
    //{
    //    NonGeneric,
    //    Generic,
    //    RuntimeTyped,
    //    Json
    //}
}

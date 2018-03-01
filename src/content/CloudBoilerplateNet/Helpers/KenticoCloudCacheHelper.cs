using System.Collections.Generic;

using CloudBoilerplateNet.Services;

namespace CloudBoilerplateNet.Helpers
{
    public class KenticoCloudCacheHelper : IRelatedTypesResolver
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
        public const string CONTENT_TYPE_JSON_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_TYPE_LISTING_IDENTIFIER = CONTENT_TYPE_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string CONTENT_TYPE_LISTING_JSON_IDENTIFIER = CONTENT_TYPE_LISTING_IDENTIFIER + JSON_SUFFIX;
        public const string CONTENT_ELEMENT_IDENTIFIER = "content_element";
        public const string TAXONOMY_GROUP_SINGLE_IDENTIFIER = "taxonomy";
        public const string TAXONOMY_GROUP_JSON_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + LISTING_SUFFIX;
        public const string TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER = TAXONOMY_GROUP_LISTING_IDENTIFIER + JSON_SUFFIX;

        static IEnumerable<string> ContentItemSingleRelatedTypes
        {
            get
            {
                return new List<string>
                {
                    CONTENT_ITEM_SINGLE_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_JSON_IDENTIFIER,
                    CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER
                };
            }
        }

        static IEnumerable<string> ContentItemListingRelatedTypes
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

        public IEnumerable<string> GetRelatedTypes(string typeCodeName)
        {
            switch (typeCodeName)
            {
                case CONTENT_ITEM_SINGLE_IDENTIFIER:
                case CONTENT_ITEM_SINGLE_JSON_IDENTIFIER:
                case CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER:
                case CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER:
                case CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER:
                    return ContentItemSingleRelatedTypes;
                case CONTENT_ITEM_LISTING_IDENTIFIER:
                case CONTENT_ITEM_LISTING_JSON_IDENTIFIER:
                case CONTENT_ITEM_LISTING_TYPED_IDENTIFIER:
                case CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER:
                    return ContentItemListingRelatedTypes;
                default:
                    return null;
            }
        }


    }
}

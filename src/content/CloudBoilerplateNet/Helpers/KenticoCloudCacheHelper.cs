using System;
using System.Collections.Generic;
using System.Linq;
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
        public const string CONTENT_ELEMENT_JSON_IDENTIFIER = CONTENT_ELEMENT_IDENTIFIER + JSON_SUFFIX;
        public const string TAXONOMY_GROUP_SINGLE_IDENTIFIER = "taxonomy";
        public const string TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER = TAXONOMY_GROUP_SINGLE_IDENTIFIER + JSON_SUFFIX;
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
                    CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER,
                    CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER,
                    CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER,
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

        static IEnumerable<string> ContentTypeSingleRelatedTypes
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

        static IEnumerable<string> ContentTypeListingRelatedTypes
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

        static IEnumerable<string> ContentElementRelatedTypes
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

        static IEnumerable<string> TaxonomyGroupSingleRelatedTypes
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

        static IEnumerable<string> TaxonomyGroupListingRelatedTypes
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

        public IEnumerable<string> GetRelatedTypes(string typeCodeName)
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
    }
}

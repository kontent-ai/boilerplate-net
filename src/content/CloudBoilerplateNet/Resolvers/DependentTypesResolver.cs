using System;
using System.Collections.Generic;
using System.Linq;

using CloudBoilerplateNet.Helpers;

namespace CloudBoilerplateNet.Resolvers
{
    public class DependentFormatResolver : IDependentTypesResolver
    {
        public Dictionary<string, IEnumerable<string>> RelatedFormats
        {
            get => new Dictionary<string, IEnumerable<string>>
            {
                { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, KenticoCloudCacheHelper.ContentItemSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER, KenticoCloudCacheHelper.ContentItemSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER, KenticoCloudCacheHelper.ContentItemSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER, KenticoCloudCacheHelper.ContentItemSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, KenticoCloudCacheHelper.ContentItemSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_IDENTIFIER, KenticoCloudCacheHelper.ContentItemListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER, KenticoCloudCacheHelper.ContentItemListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_TYPED_IDENTIFIER, KenticoCloudCacheHelper.ContentItemListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER, KenticoCloudCacheHelper.ContentItemListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER, KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_TYPE_LISTING_IDENTIFIER, KenticoCloudCacheHelper.ContentTypeListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_TYPE_LISTING_JSON_IDENTIFIER, KenticoCloudCacheHelper.ContentTypeListingRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, KenticoCloudCacheHelper.ContentElementRelatedFormats },
                { KenticoCloudCacheHelper.CONTENT_ELEMENT_JSON_IDENTIFIER, KenticoCloudCacheHelper.ContentElementRelatedFormats },
                { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats },
                { KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER, KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats },
                { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_IDENTIFIER, KenticoCloudCacheHelper.TaxonomyGroupListingRelatedFormats },
                { KenticoCloudCacheHelper.TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER, KenticoCloudCacheHelper.TaxonomyGroupListingRelatedFormats }
            };
        }

        public IEnumerable<string> GetDependentTypeNames(string typeCodeName)
        {
            return RelatedFormats[typeCodeName];
        }
    }
}

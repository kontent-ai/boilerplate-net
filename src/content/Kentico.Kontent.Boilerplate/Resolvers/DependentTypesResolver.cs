using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Kontent.Boilerplate.Helpers;

namespace Kentico.Kontent.Boilerplate.Resolvers
{
    public class DependentFormatResolver : IDependentTypesResolver
    {
        public Dictionary<string, IEnumerable<string>> RelatedFormats
        {
            get => new Dictionary<string, IEnumerable<string>>
            {
                { CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, CacheHelper.ContentItemSingleRelatedFormats },
                { CacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER, CacheHelper.ContentItemSingleRelatedFormats },
                { CacheHelper.CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER, CacheHelper.ContentItemSingleRelatedFormats },
                { CacheHelper.CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER, CacheHelper.ContentItemSingleRelatedFormats },
                { CacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, CacheHelper.ContentItemSingleRelatedFormats },
                { CacheHelper.CONTENT_ITEM_LISTING_IDENTIFIER, CacheHelper.ContentItemListingRelatedFormats },
                { CacheHelper.CONTENT_ITEM_LISTING_JSON_IDENTIFIER, CacheHelper.ContentItemListingRelatedFormats },
                { CacheHelper.CONTENT_ITEM_LISTING_TYPED_IDENTIFIER, CacheHelper.ContentItemListingRelatedFormats },
                { CacheHelper.CONTENT_ITEM_LISTING_RUNTIME_TYPED_IDENTIFIER, CacheHelper.ContentItemListingRelatedFormats },
                { CacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, CacheHelper.ContentTypeSingleRelatedFormats },
                { CacheHelper.CONTENT_TYPE_SINGLE_JSON_IDENTIFIER, CacheHelper.ContentTypeSingleRelatedFormats },
                { CacheHelper.CONTENT_TYPE_LISTING_IDENTIFIER, CacheHelper.ContentTypeListingRelatedFormats },
                { CacheHelper.CONTENT_TYPE_LISTING_JSON_IDENTIFIER, CacheHelper.ContentTypeListingRelatedFormats },
                { CacheHelper.CONTENT_ELEMENT_IDENTIFIER, CacheHelper.ContentElementRelatedFormats },
                { CacheHelper.CONTENT_ELEMENT_JSON_IDENTIFIER, CacheHelper.ContentElementRelatedFormats },
                { CacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, CacheHelper.TaxonomyGroupSingleRelatedFormats },
                { CacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER, CacheHelper.TaxonomyGroupSingleRelatedFormats },
                { CacheHelper.TAXONOMY_GROUP_LISTING_IDENTIFIER, CacheHelper.TaxonomyGroupListingRelatedFormats },
                { CacheHelper.TAXONOMY_GROUP_LISTING_JSON_IDENTIFIER, CacheHelper.TaxonomyGroupListingRelatedFormats }
            };
        }

        public IEnumerable<string> GetDependentTypeNames(string typeCodeName)
        {
            return RelatedFormats[typeCodeName];
        }
    }
}

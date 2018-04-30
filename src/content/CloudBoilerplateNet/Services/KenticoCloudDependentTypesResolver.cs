using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudBoilerplateNet.Helpers;

namespace CloudBoilerplateNet.Services
{
    public class KenticoCloudDependentFormatResolver : IDependentTypesResolver
    {
        public IEnumerable<string> GetDependentTypeNames(string typeCodeName)
        {
            if (KenticoCloudCacheHelper.ContentItemSingleRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentItemListingRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemListingRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentTypeListingRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeListingRelatedFormats;
            }
            else if (typeCodeName.Equals(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, StringComparison.Ordinal))
            {
                return KenticoCloudCacheHelper.ContentElementRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupListingRelatedFormats.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.TaxonomyGroupListingRelatedFormats;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}

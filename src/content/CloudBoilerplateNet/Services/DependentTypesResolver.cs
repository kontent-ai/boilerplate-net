using System;
using System.Collections.Generic;
using System.Linq;

using CloudBoilerplateNet.Helpers;

namespace CloudBoilerplateNet.Services
{
    public class DependentFormatResolver : IDependentTypesResolver
    {
        public IEnumerable<string> GetDependentTypeNames(string typeCodeName)
        {
            if (KenticoCloudCacheHelper.ContentItemSingleRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentItemListingRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemListingRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentTypeListingRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeListingRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.ContentElementRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentElementRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedFormats;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupListingRelatedFormats.Any(rf => typeCodeName.Equals(rf, StringComparison.Ordinal)))
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

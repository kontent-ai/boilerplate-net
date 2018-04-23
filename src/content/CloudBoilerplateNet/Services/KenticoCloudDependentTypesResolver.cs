using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudBoilerplateNet.Helpers;

namespace CloudBoilerplateNet.Services
{
    public class KenticoCloudDependentTypesResolver : IDependentTypesResolver
    {
        public IEnumerable<string> GetDependentTypeNames(string typeCodeName)
        {
            if (KenticoCloudCacheHelper.ContentItemSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemSingleRelatedTypes;
            }
            else if (KenticoCloudCacheHelper.ContentItemListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentItemListingRelatedTypes;
            }
            else if (KenticoCloudCacheHelper.ContentTypeSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeSingleRelatedTypes;
            }
            else if (KenticoCloudCacheHelper.ContentTypeListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.ContentTypeListingRelatedTypes;
            }
            else if (typeCodeName.Equals(KenticoCloudCacheHelper.CONTENT_ELEMENT_IDENTIFIER, StringComparison.Ordinal))
            {
                return KenticoCloudCacheHelper.ContentElementRelatedTypes;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.TaxonomyGroupSingleRelatedTypes;
            }
            else if (KenticoCloudCacheHelper.TaxonomyGroupListingRelatedTypes.Any(rt => typeCodeName.Equals(rt, StringComparison.Ordinal)))
            {
                return KenticoCloudCacheHelper.TaxonomyGroupListingRelatedTypes;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}

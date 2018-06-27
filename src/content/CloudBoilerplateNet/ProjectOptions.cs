using System.Linq;
using KenticoCloud.Delivery;

namespace CloudBoilerplateNet
{
    public class ProjectOptions
    {
        public DeliveryOptions DeliveryOptions { get; set; }

        public int CacheTimeoutSeconds { get; set; }

        public string KenticoCloudWebhookSecret { get; set; }

        public bool CreateCacheEntriesInBackground { get; set; }

        public int[] ResponsiveWidths { get; set; }

        public bool ResponsiveImagesEnabled
        {
            get
            {
                return ResponsiveWidths != null && ResponsiveWidths.Count() > 0;
            }
        }
    }
}

using System.Linq;
using Kentico.Kontent.Delivery;

namespace Kentico.Kontent.Boilerplate
{
    public class ProjectOptions
    {
        public DeliveryOptions DeliveryOptions { get; set; }

        public int CacheTimeoutSeconds { get; set; }

        public string KenticoKontentWebhookSecret { get; set; }

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

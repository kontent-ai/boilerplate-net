using System.Linq;
using Kentico.Kontent.Delivery;

namespace Kentico.Kontent.Boilerplate
{
    public class ProjectOptions
    {
        public DeliveryOptions DeliveryOptions { get; set; }

        public string KenticoKontentWebhookSecret { get; set; }

        public int[] ResponsiveWidths { get; set; }

        public bool ResponsiveImagesEnabled => ResponsiveWidths != null && ResponsiveWidths.Any();
    }
}

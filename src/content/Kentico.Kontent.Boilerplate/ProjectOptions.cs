using System.Linq;
using Kentico.Kontent.AspNetCore.Middleware.Webhook;
using Kentico.Kontent.Delivery;

namespace Kentico.Kontent.Boilerplate
{
    public class ProjectOptions
    {
        public DeliveryOptions DeliveryOptions { get; set; }

        public WebhookOptions WebhookOptions { get; set; }

        public int[] ResponsiveWidths { get; set; }
    }
}

using KenticoCloud.Delivery;

namespace CloudBoilerplateNet
{
    public class ProjectOptions
    {
        public DeliveryOptions DeliveryOptions { get; set; }

        public int CacheTimeoutSeconds { get; set; }

        public string KenticoCloudWebhookSecret { get; set; }
    }
}

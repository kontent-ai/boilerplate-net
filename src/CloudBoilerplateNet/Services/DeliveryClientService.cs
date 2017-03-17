using CloudBoilerplateNet.Interfaces;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Options;

namespace CloudBoilerplateNet.Services
{
    public class DeliveryClientService : IDeliveryClientService
    {
        public DeliveryClient Client { get; }

        public DeliveryClientService(IOptions<ProjectOptions> projectOptions)
        {
            if (string.IsNullOrEmpty(projectOptions.Value.KenticoCloudPreviewApiKey))
            {
                Client = new DeliveryClient(projectOptions.Value.KenticoCloudProjectId);
            }
            else
            {
                Client = new DeliveryClient(
                    projectOptions.Value.KenticoCloudProjectId,
                    projectOptions.Value.KenticoCloudPreviewApiKey
                );
            }

            Client.CodeFirstModelProvider.TypeProvider = new CustomTypeProvider();
        }
    }
}

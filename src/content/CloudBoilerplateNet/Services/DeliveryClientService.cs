using CloudBoilerplateNet.Interfaces;
using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CloudBoilerplateNet.Services
{
    public class DeliveryClientService : IDeliveryClientService
    {
        public IDeliveryClient Client { get; }

        public DeliveryClientService(IOptions<ProjectOptions> projectOptions, IMemoryCache memoryCache)
        {
            Client = new CachedDeliveryClient(projectOptions, memoryCache, 60);

            Client.CodeFirstModelProvider.TypeProvider = new CustomTypeProvider();
        }
    }
}

using CloudBoilerplateNet.Controllers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace CloudBoilerplateNet.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void IndexTests()
        {
            var projectOptions = Options.Create(new ProjectOptions()
            {
                KenticoCloudProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3"
            });

            var memoryCacheOptions = Options.Create(new MemoryCacheOptions()
            {
                Clock = new TestClock(),
                CompactOnMemoryPressure = true,
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });

            var deliveryClientService = new DeliveryClientService(projectOptions, new MemoryCache(memoryCacheOptions));
            var controller = new HomeController(deliveryClientService);
            var result = Task.Run(() => controller.Index()).Result;
            
            Assert.Equal(typeof(ReadOnlyCollection<Article>), result.Model.GetType());
        }
    }
}

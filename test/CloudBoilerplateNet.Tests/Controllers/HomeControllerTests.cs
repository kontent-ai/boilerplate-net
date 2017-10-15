using CloudBoilerplateNet.Controllers;
using CloudBoilerplateNet.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CloudBoilerplateNet.Services;
using Xunit;
using KenticoCloud.Delivery;

namespace CloudBoilerplateNet.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void IndexTests()
        {
            var projectOptions = Options.Create(new ProjectOptions
            {
                CacheTimeoutSeconds = 60,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3"
                }
            });


            var memoryCacheOptions = Options.Create(new MemoryCacheOptions
            {
                Clock = new TestClock(),
                CompactOnMemoryPressure = true,
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });

            var deliveryClient = new CachedDeliveryClient(projectOptions, new MemoryCache(memoryCacheOptions));
            var controller = new HomeController(deliveryClient);
            var result = Task.Run(() => controller.Index()).Result;

            Assert.Equal(typeof(ReadOnlyCollection<Article>), result.Model.GetType());
        }
    }
}

using Microsoft.Extensions.Options;
using Xunit;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Caching.Memory;
using System;
using CloudBoilerplateNet.Services;
using CloudBoilerplateNet.Controllers;
using CloudBoilerplateNet.Models.ContentTypes;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Tests
{
    public class WebhookControllerTests
    {
        private IOptions<ProjectOptions> projectOptions;
        private IOptions<MemoryCacheOptions> memoryCacheOptions;

        public WebhookControllerTests()
        {
            projectOptions = Options.Create(new ProjectOptions
            {
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3",
                    WaitForLoadingNewContent = true
                },
                CacheTimeoutSeconds = 60,
                WebhookSecret = "ga6jrBQtHPhPC0C8nXN7AIzOS3L9cv8dQBRatnJO62l"
            });

            memoryCacheOptions = Options.Create(new MemoryCacheOptions
            {
                Clock = new TestClock(),
                CompactOnMemoryPressure = true,
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });
        }

        [Fact]
        public async Task PostTests()
        {
            var deliveryClient = new CachedDeliveryClient(projectOptions, new MemoryCache(memoryCacheOptions));
            WebhookController controller = new WebhookController(projectOptions, deliveryClient);
            HttpRequestMessage request = CreateParameterForPost();
            IActionResult result = await controller.Post(request);
            Assert.IsType(typeof(AcceptedResult), result);
        }

        private HttpRequestMessage CreateParameterForPost()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            KenticoCloudWebhookModel kenticoCloudWebhookModel = CreateMockObject();
            string json = JsonConvert.SerializeObject(kenticoCloudWebhookModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;

            string hash = GenerateHash(json);
            request.Headers.Add("X-KC-Signature", hash);

            return request;
        }

        private KenticoCloudWebhookModel CreateMockObject()
        {

            KenticoCloudWebhookModel kenticoCloudWebhookModel = new KenticoCloudWebhookModel();

            Data data = new Data();
            Item item1 = new Item()
            {
                Language = "en-US",
                Codename = "a_chemex_method",
                Type = "hosted_video"
            };

            Item item2 = new Item()
            {
                Language = "en-US",
                Codename = "which_brewing_fits_you_",
                Type = "article"
            };

            data.Items = new Item[] { item1, item2 };
            kenticoCloudWebhookModel.Data = data;

            Message message = new Message
            {
                Id = Guid.NewGuid(),
            };
            kenticoCloudWebhookModel.Message = message;

            return kenticoCloudWebhookModel;
        }

        private string GenerateHash(string message)
        {
            string secret = "ga6jrBQtHPhPC0C8nXN7AIzOS3L9cv8dQBRatnJO62l";
            UTF8Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            byte[] keyBytes = SafeUTF8.GetBytes(secret);
            byte[] messageBytes = SafeUTF8.GetBytes(message);
            using (System.Security.Cryptography.HMACSHA256 hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
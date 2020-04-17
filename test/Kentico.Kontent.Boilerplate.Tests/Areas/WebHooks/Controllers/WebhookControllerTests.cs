using System.Threading.Tasks;
using Kentico.Kontent.AspNetCore.Webhooks.Models;
using Kentico.Kontent.Boilerplate.Areas.WebHooks.Controllers;
using Kentico.Kontent.Delivery.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kentico.Kontent.Boilerplate.Tests.Areas.WebHooks.Controllers
{
    public enum PayloadType
    {
        Items,
        Taxonomies
    }

    public class WebhookControllerTests
    {
        [Theory]
        [InlineData(PayloadType.Items, "content_item", "article", "upsert")]
        [InlineData(PayloadType.Items, "content_item_variant", "article", "upsert")]
        [InlineData(PayloadType.Items, "content_type", "article", "upsert")]
        [InlineData(PayloadType.Taxonomies, "taxonomy", "personas", "upsert")]
        [InlineData(PayloadType.Items, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Taxonomies, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Items, "content_item", "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, "content_item_variant", "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, "content_type", "article", "lorem_ipsum")]
        [InlineData(PayloadType.Taxonomies, "taxonomy", "personas", "lorem_ipsum")]
        [InlineData(PayloadType.Items, "lorem_ipsum", "dolor_sit_amet", "lorem_ipsum")]
        [InlineData(PayloadType.Taxonomies, "lorem_ipsum", "dolor_sit_amet", "lorem_ipsum")]
        public async Task ReturnsOkWheneverPossible(PayloadType payloadType, string artefactType, string dataType, string operation)
        {
            Item[] items = null;
            Taxonomy[] taxonomies = null;

            switch (payloadType)
            {
                case PayloadType.Items:
                    items = new[] { new Item { Codename = "Test", Type = dataType } };
                    break;
                case PayloadType.Taxonomies:
                    taxonomies = new[] { new Taxonomy { Codename = "Test" } };
                    break;
            }

            var model = new WebhookModel
            {
                Data = new Data { Items = items, Taxonomies = taxonomies },
                Message = new Message { Type = artefactType, Operation = operation }
            };
            
            var controller = new WebhooksController(new DeliveryCacheManager(new MemoryCache(Options.Create(new MemoryCacheOptions())), Options.Create(new DeliveryCacheOptions())));
            var result = (StatusCodeResult)await Task.Run(() => controller.Index(model));

            Assert.InRange(result.StatusCode, 200, 299);
        }
    }
}

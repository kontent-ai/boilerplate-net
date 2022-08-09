using System.Threading.Tasks;
using FakeItEasy;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kontent.Ai.Boilerplate.Tests.Areas.WebHooks.Controllers
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
            DeliveryWebhookItem[] items = null;
            Taxonomy[] taxonomies = null;

            switch (payloadType)
            {
                case PayloadType.Items:
                    items = new[] { new DeliveryWebhookItem { Codename = "Test", Type = dataType } };
                    break;
                case PayloadType.Taxonomies:
                    taxonomies = new[] { new Taxonomy { Codename = "Test" } };
                    break;
            }

            var model = new DeliveryWebhookModel 
            {
                Data = new DeliveryWebhookData { Items = items, Taxonomies = taxonomies },
                Message = new Message { Type = artefactType, Operation = operation }
            };

            var cacheManger = A.Fake<IDeliveryCacheManager>();
            var controller = new WebhooksController(cacheManger);
            var result = (StatusCodeResult)await controller.Index(model);

            Assert.InRange(result.StatusCode, 200, 299);
        }
    }
}

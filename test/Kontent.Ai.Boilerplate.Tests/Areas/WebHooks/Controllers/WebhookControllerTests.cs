using System;
using System.Threading.Tasks;
using FakeItEasy;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kontent.Ai.Boilerplate.Tests.Areas.WebHooks.Controllers
{
    public class WebhookControllerTests
    {
        [Theory]
        [InlineData("content_item", "article", "upsert")]
        [InlineData("content_item_variant", "article", "upsert")]
        [InlineData("content_type", "article", "upsert")]
        [InlineData("taxonomy", "personas", "upsert")]
        [InlineData("lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData("content_item", "article", "lorem_ipsum")]
        [InlineData("content_item_variant", "article", "lorem_ipsum")]
        [InlineData("content_type", "article", "lorem_ipsum")]
        [InlineData("taxonomy", "personas", "lorem_ipsum")]
        [InlineData("lorem_ipsum", "dolor_sit_amet", "lorem_ipsum")]
        public async Task ReturnsOkWheneverPossible(string objectType, string codename, string action)
        {
            var webhook = new WebhookNotification
            {
                Notifications =
                [
                    new WebhookModel
                    {
                        Data = new WebhookData
                        {
                            System = new WebhookItem
                            {
                                Codename = codename
                            }
                        },
                        Message = new WebhookMessage
                        {
                            ObjectType = objectType,
                            Action = action,
                            DeliverySlot = "published",
                            EnvironmentId = Guid.NewGuid()
                        }
                    }
                ]
            };

            var cacheManger = A.Fake<IDeliveryCacheManager>();
            var controller = new WebhooksController(cacheManger);
            var result = (StatusCodeResult)await controller.Index(webhook);

            Assert.InRange(result.StatusCode, 200, 299);
        }
    }
}

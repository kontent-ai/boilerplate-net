using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kontent.Ai.Boilerplate.Tests.Areas.WebHooks.Controllers
{
    public class WebhookControllerTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_NullCacheManager_ThrowsArgumentNullException()
        {
            var deliveryClient = A.Fake<IDeliveryClient>();
            
            Assert.Throws<ArgumentNullException>("cacheManager", () => new WebhooksController(null, deliveryClient));
        }

        [Fact]
        public void Constructor_NullDeliveryClient_ThrowsArgumentNullException()
        {
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            
            Assert.Throws<ArgumentNullException>("deliveryClient", () => new WebhooksController(cacheManager, null));
        }

        #endregion

        #region Basic Functionality Tests

        [Fact]
        public async Task Index_NullWebhook_ReturnsOk()
        {
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();
            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(null);

            Assert.IsType<OkResult>(result);
            A.CallTo(cacheManager).MustNotHaveHappened();
        }

        [Fact]
        public async Task Index_EmptyNotifications_ReturnsOk()
        {
            var webhook = new WebhookNotification { Notifications = [] };
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();
            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            A.CallTo(cacheManager).MustNotHaveHappened();
        }

        [Fact]
        public async Task Index_NullNotifications_ReturnsOk()
        {
            var webhook = new WebhookNotification { Notifications = null };
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();
            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            A.CallTo(cacheManager).MustNotHaveHappened();
        }

        #endregion

        #region Content Item Tests

        [Theory]
        [InlineData("published")]
        [InlineData("unpublished")]
        public async Task Index_ContentItemPublishedOrUnpublished_ReturnsOk(string action)
        {
            var codename = "test-article";
            var webhook = CreateWebhook("content_item", codename, action);
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();
            
            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
        }

        [Theory]
        [InlineData("created")]
        [InlineData("updated")]
        [InlineData("deleted")]
        public async Task Index_ContentItemOtherActions_ReturnsOk(string action)
        {
            var codename = "test-article";
            var webhook = CreateWebhook("content_item", codename, action);
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region Taxonomy Tests

        [Fact]
        public async Task Index_TaxonomyCreated_InvalidatesBasicTaxonomyDependenciesOnly()
        {
            var codename = "test-taxonomy";
            var webhook = CreateWebhook("taxonomy", codename, "created");
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            
            // Verify only taxonomy-specific invalidations
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains(codename))).MustHaveHappenedOnceExactly();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("taxonomy_group_listing"))).MustHaveHappenedOnceExactly();
            
            // Verify items and types are NOT invalidated for created action
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("item_listing"))).MustNotHaveHappened();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("type_listing"))).MustNotHaveHappened();
        }

        [Theory]
        [InlineData("updated")]
        [InlineData("deleted")]
        public async Task Index_TaxonomyOtherActions_InvalidatesAllRelatedDependencies(string action)
        {
            var codename = "test-taxonomy";
            var webhook = CreateWebhook("taxonomy", codename, action);
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            
            // Verify all related invalidations
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains(codename))).MustHaveHappenedOnceExactly();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("taxonomy_group_listing"))).MustHaveHappenedOnceExactly();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("item_listing"))).MustHaveHappenedOnceExactly();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("type_listing"))).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Content Type Tests

        [Fact]
        public async Task Index_ContentType_InvalidatesTypesAndItemsDependencies()
        {
            var webhook = CreateWebhook("content_type", "article", "updated");
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            
            // Verify types and items invalidations
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("type_listing"))).MustHaveHappenedOnceExactly();
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("item_listing"))).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Language Tests

        [Fact]
        public async Task Index_Language_InvalidatesLanguagesDependencies()
        {
            var webhook = CreateWebhook("language", "en-us", "updated");
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            
            // Verify languages invalidation
            A.CallTo(() => cacheManager.InvalidateDependencyAsync(A<string>.That.Contains("language_listing"))).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Unknown Object Type Tests

        [Theory]
        [InlineData("asset")]
        [InlineData("unknown_type")]
        [InlineData("")]
        public async Task Index_UnknownObjectType_ReturnsOkWithoutInvalidation(string objectType)
        {
            var webhook = CreateWebhook(objectType, "test-codename", "updated");
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
            
            // Verify no cache invalidations
            A.CallTo(cacheManager).MustNotHaveHappened();
        }

        #endregion

        #region Multiple Notifications Tests

        [Fact]
        public async Task Index_MultipleNotifications_ProcessesAllCorrectly()
        {
            var webhook = new WebhookNotification
            {
                Notifications =
                [
                    CreateNotification("content_item", "article1", "published"),
                    CreateNotification("taxonomy", "categories", "updated"),
                    CreateNotification("content_type", "article", "created"),
                    CreateNotification("language", "en-us", "updated")
                ]
            };
            
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task Index_AllObjectTypes_ReturnsOk()
        {
            var webhook = new WebhookNotification
            {
                Notifications =
                [
                    CreateNotification("content_item", "article", "published"),
                    CreateNotification("taxonomy", "categories", "created"),
                    CreateNotification("content_type", "article", "updated"),
                    CreateNotification("language", "en-us", "updated"),
                    CreateNotification("asset", "image", "updated") // Unknown type - should be ignored
                ]
            };
            
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();

            var controller = new WebhooksController(cacheManager, deliveryClient);

            var result = await controller.Index(webhook);

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region Helper Methods

        private static WebhookNotification CreateWebhook(string objectType, string codename, string action)
        {
            return new WebhookNotification
            {
                Notifications = [CreateNotification(objectType, codename, action)]
            };
        }

        private static WebhookModel CreateNotification(string objectType, string codename, string action)
        {
            return new WebhookModel
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
            };
        }

        private static IUsedInItem CreateUsedInItem(string codename)
        {
            var item = A.Fake<IUsedInItem>();
            var system = A.Fake<IUsedInItemSystemAttributes>();
            A.CallTo(() => system.Codename).Returns(codename);
            A.CallTo(() => item.System).Returns(system);
            return item;
        }

        #endregion

        #region Legacy Test (keeping for compatibility)

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
            var webhook = CreateWebhook(objectType, codename, action);
            var cacheManager = A.Fake<IDeliveryCacheManager>();
            var deliveryClient = A.Fake<IDeliveryClient>();
            var controller = new WebhooksController(cacheManager, deliveryClient);
            
            var result = (StatusCodeResult)await controller.Index(webhook);

            Assert.InRange(result.StatusCode, 200, 299);
        }

        #endregion
    }
}

using System.Threading.Tasks;
using Kentico.Kontent.Boilerplate.Areas.WebHooks.Controllers;
using Kentico.Kontent.Boilerplate.Areas.WebHooks.Models;
using Kentico.Kontent.Boilerplate.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kentico.Kontent.Boilerplate.Tests.Areas.WebHooks.Controllers
{
    public enum PayloadType
    {
        Items,
        Taxonomies
    }

    public class KenticoKontentControllerTests
    {
        [Theory]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Taxonomies, CacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, "personas", "upsert")]
        [InlineData(PayloadType.Items, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Taxonomies, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, CacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Taxonomies, CacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, "personas", "lorem_ipsum")]
        [InlineData(PayloadType.Items, "lorem_ipsum", "dolor_sit_amet", "lorem_ipsum")]
        [InlineData(PayloadType.Taxonomies, "lorem_ipsum", "dolor_sit_amet", "lorem_ipsum")]
        public void ReturnsOkWheneverPossible(PayloadType payloadType, string artefactType, string dataType, string operation)
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
                default:
                    break;
            }

            var model = new KenticoKontentWebhookModel
            {
                Data = new Data { Items = items, Taxonomies = taxonomies },
                Message = new Message { Type = artefactType, Operation = operation }
            };
            
            var controller = new KenticoKontentController(new WebhookListener());
            var result = (StatusCodeResult)Task.Run(() => controller.Index(model)).Result;

            Assert.InRange(result.StatusCode, 200, 299);
        }
    }
}

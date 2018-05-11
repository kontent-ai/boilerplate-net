using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using CloudBoilerplateNet.Areas.WebHooks.Controllers;
using CloudBoilerplateNet.Areas.WebHooks.Models;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CloudBoilerplateNet.Tests.Areas.WebHooks.Controllers
{
    public enum PayloadType
    {
        Items,
        Taxonomies
    }

    public class KenticoCloudControllerTests
    {
        [Theory]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, "article", "upsert")]
        [InlineData(PayloadType.Taxonomies, KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, "personas", "upsert")]
        [InlineData(PayloadType.Items, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Taxonomies, "lorem_ipsum", "dolor_sit_amet", "upsert")]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER, "article", "lorem_ipsum")]
        [InlineData(PayloadType.Taxonomies, KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER, "personas", "lorem_ipsum")]
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

            var model = new KenticoCloudWebhookModel
            {
                Data = new Data { Items = items, Taxonomies = taxonomies },
                Message = new Message { Type = artefactType, Operation = operation }
            };
            
            var controller = new KenticoCloudController(new WebhookListener());
            var result = (StatusCodeResult)Task.Run(() => controller.Index(model)).Result;

            Assert.InRange(result.StatusCode, 200, 299);
        }
    }
}

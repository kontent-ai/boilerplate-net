using System;
using System.Collections.Generic;
using System.Text;

using CloudBoilerplateNet.Areas.WebHooks.Models;
using CloudBoilerplateNet.Helpers;
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
        [InlineData(new KenticoCloudWebhookModel
        {
            Data = new Data { Items = new[] { new Item { Codename = "Test", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER } } },
            Message = new Message { Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, Operation = "upsert" }
        })]
        [InlineData(PayloadType.Items, KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER, )]
        public void ReturnsOkWhenPossible(PayloadType payloadType, string artefactType, string dataType)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Caching.Memory;
using KenticoCloud.Delivery;
using CloudBoilerplateNet.Controllers;
using CloudBoilerplateNet.Filters;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using CloudBoilerplateNet.Areas.WebHooks.Models;

namespace CloudBoilerplateNet.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class KenticoCloudController : BaseController
    {
        protected List<string> PurgingOperations => new List<string>()
        {
            "archive",
            "publish",
            "unpublish",
            "upsert"
        };

        protected IWebhookObservableProvider KenticoCloudWebhookObservableProvider { get; }
        protected ICacheManager CacheManager { get; }

        public KenticoCloudController(IDeliveryClient deliveryClient, IWebhookObservableProvider kenticoCloudWebhookObservableProvider, ICacheManager cacheManager) : base(deliveryClient)
        {
            KenticoCloudWebhookObservableProvider = kenticoCloudWebhookObservableProvider;
            CacheManager = cacheManager;
        }

        [HttpPost]
        // TODO: Uncomment [ServiceFilter(typeof(KenticoCloudSignatureActionFilter))]
        public IActionResult Index([FromBody] KenticoCloudWebhookModel model)
        {
            switch (model.Message.Type)
            {
                case KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER:
                case KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER:
                case KenticoCloudCacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER:
                    return RaiseNotificationForSupportedOperations(model.Message.Operation, model.Message.Type, model.Data.Items);
                case KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER:
                    return RaiseNotificationForSupportedOperations(model.Message.Operation, model.Message.Type, model.Data.Taxonomies);
                default:
                    // For all other types of artifacts, return OK to avoid webhook re-submissions.
                    return Ok();
            }
        }

        private IActionResult RaiseNotificationForSupportedOperations(string operation, string type, IEnumerable<ICodenamedData> data)
        {
            if (PurgingOperations.Any(o => o.Equals(operation, StringComparison.Ordinal)))
            {
                foreach (var item in data)
                {
                    KenticoCloudWebhookObservableProvider.RaiseWebhookNotification(
                        new IdentifierSet
                        {
                            Type = type,
                            Codename = item.Codename
                        });
                }

                return Ok();
            }
            else
            {
                // For all other operations, return OK to avoid webhook re-submissions.
                return Ok();
            }
        }
    }
}

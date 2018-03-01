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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CloudBoilerplateNet.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class KenticoCloudController : BaseController
    {
        protected IWebhookObservableProvider KenticoCloudWebhookObservableProvider { get; }
        protected ICacheManager CacheManager { get; }

        public KenticoCloudController(IDeliveryClient deliveryClient, IWebhookObservableProvider kenticoCloudWebhookObservableProvider, ICacheManager cacheManager) : base(deliveryClient)
        {
            KenticoCloudWebhookObservableProvider = kenticoCloudWebhookObservableProvider;
            CacheManager = cacheManager;
        }

        [HttpPost]
        [ServiceFilter(typeof(KenticoCloudSignatureActionFilter))]
        public IActionResult Index([FromBody] KenticoCloudWebhookModel model)
        {
            switch (model.Message.Type)
            {
                case KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER:
                case KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER:
                    switch (model.Message.Operation)
                    {
                        case "archive":
                        case "publish":
                        case "unpublish":
                        case "upsert":
                            foreach (var item in model.Data.Items)
                            {
                                KenticoCloudWebhookObservableProvider.RaiseWebhookNotification(
                                    new KenticoCloudCacheHelper(),
                                    new IdentifierSet
                                    {
                                        Type = model.Message.Type,
                                        Codename = item.Codename
                                    });
                            }

                            break;
                        default:
                            return Ok();
                    }

                    // For all other operations, return OK to avoid webhook re-submissions.
                    return Ok();
                default:
                    // For all other types of artifacts, return OK, for the same reason as above.
                    return Ok();
            }
        }
    }
}

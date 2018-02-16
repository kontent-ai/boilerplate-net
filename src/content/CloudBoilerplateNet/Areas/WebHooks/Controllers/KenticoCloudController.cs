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
    public class KenticoCloudController : BaseController
    {
        protected ICacheManager CacheManager { get; }

        public KenticoCloudController(IDeliveryClient deliveryClient, ICacheManager cacheManager) : base(deliveryClient) => CacheManager = cacheManager;

        [HttpPost]
        [ServiceFilter(typeof(KenticoCloudSignatureActionFilter))]
        public IActionResult Index([FromBody] KenticoCloudWebhookModel model)
        {
            switch (model.Message.Type)
            {
                case CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER:
                case CacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER:
                    switch (model.Message.Operation)
                    {
                        case "archive":
                        case "publish":
                        case "unpublish":
                        case "upsert":
                            foreach (var item in model.Data.Items)
                            {
                                CacheManager.InvalidateEntry(new IdentifierSet
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

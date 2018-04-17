using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
        protected List<string> SupportedOperations => new List<string>()
        {
            "upsert",
            "publish",
            "restore_publish",
            "unpublish",
            "archive",
            "restore"
        };

        protected ICacheManager CacheManager { get; }
        protected IKenticoCloudWebhookListener KenticoCloudWebhookListener { get; }

        public KenticoCloudController(IDeliveryClient deliveryClient, ICacheManager cacheManager, IKenticoCloudWebhookListener kenticoCloudWebhookListener) : base(deliveryClient)
        {
            CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            KenticoCloudWebhookListener = kenticoCloudWebhookListener ?? throw new ArgumentNullException(nameof(kenticoCloudWebhookListener));
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
            if (SupportedOperations.Any(o => o.Equals(operation, StringComparison.Ordinal)))
            {
                foreach (var item in data)
                {
                    KenticoCloudWebhookListener.RaiseWebhookNotification(
                        this,
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

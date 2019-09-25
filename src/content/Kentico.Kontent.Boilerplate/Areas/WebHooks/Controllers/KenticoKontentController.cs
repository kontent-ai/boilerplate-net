using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Kentico.Kontent.Boilerplate.Filters;
using Kentico.Kontent.Boilerplate.Helpers;
using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Boilerplate.Services;
using Kentico.Kontent.Boilerplate.Areas.WebHooks.Models;

namespace Kentico.Kontent.Boilerplate.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class KenticoKontentController : Controller
    {
        protected IWebhookListener WebhookListener { get; }

        public KenticoKontentController(IWebhookListener kenticoKontentWebhookListener)
        {
            WebhookListener = kenticoKontentWebhookListener ?? throw new ArgumentNullException(nameof(kenticoKontentWebhookListener));
        }

        [HttpPost]
        [ServiceFilter(typeof(KenticoKontentSignatureActionFilter))]
        public IActionResult Index([FromBody] KenticoKontentWebhookModel model)
        {
            switch (model.Message.Type)
            {
                case CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER:
                case CacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER:
                case CacheHelper.CONTENT_TYPE_SINGLE_IDENTIFIER:
                    return RaiseNotificationForSupportedOperations(model.Message.Operation, model.Message.Type, model.Data.Items);
                case CacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER:
                    return RaiseNotificationForSupportedOperations(model.Message.Operation, model.Message.Type, model.Data.Taxonomies);
                default:
                    // For all other types of artifacts, return OK to avoid webhook re-submissions.
                    return Ok();
            }
        }

        private IActionResult RaiseNotificationForSupportedOperations(string operation, string artefactType, IEnumerable<ICodenamedData> data)
        {
            foreach (var item in data)
            {
                WebhookListener.RaiseWebhookNotification(
                    this,
                    operation,
                    new IdentifierSet
                    {
                        Type = artefactType,
                        Codename = item.Codename
                    });
            }

            return Ok();
        }
    }
}

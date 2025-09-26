using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using System.Threading.Tasks;

namespace Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class WebhooksController(IDeliveryCacheManager cacheManager) : Controller
    {
        private readonly IDeliveryCacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookNotification? webhook)
        {
            if (webhook is null)
            {
                return Ok();
            }

            var tasks = (webhook.Notifications ?? Enumerable.Empty<WebhookModel>())
                .Select(GetDependencyKey)
                .Where(key => key is not null)
                .Distinct(StringComparer.Ordinal)
                .Select(key => _cacheManager.InvalidateDependencyAsync(key!));

            await Task.WhenAll(tasks);

            return Ok();
        }

        private static string? GetDependencyKey(WebhookModel notification) =>
            notification switch
            {
                { Message.ObjectType: "content_item", Data.System.Codename: var code } => CacheHelpers.GetItemDependencyKey(code),
                { Message.ObjectType: "taxonomy", Data.System.Codename: var code } => CacheHelpers.GetTaxonomyDependencyKey(code),
                { Message.ObjectType: "language" } => CacheHelpers.GetLanguagesDependencyKey(),
                { Message.ObjectType: "content_type" } => CacheHelpers.GetTypesDependencyKey(),
                // there's no cache helper in the SDK for asset notifications yet
                _ => null
            };
    }
}

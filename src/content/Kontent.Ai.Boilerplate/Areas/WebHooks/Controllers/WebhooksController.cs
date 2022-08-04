using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using System.Threading.Tasks;

namespace Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class WebhooksController : Controller
    {
        private readonly IDeliveryCacheManager _cacheManager;

        public WebhooksController(IDeliveryCacheManager cacheManager)
        {
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] DeliveryWebhookModel model)
        {
            if (model != null)
            {
                var dependencies = new HashSet<string>();
                if (model.Data.Items?.Any() == true)
                {
                    foreach (var item in model.Data.Items ?? Enumerable.Empty<DeliveryWebhookItem>())
                    {
                        dependencies.Add(CacheHelpers.GetItemDependencyKey(item.Codename));
                    }

                    dependencies.Add(CacheHelpers.GetItemsDependencyKey());
                }

                if (model.Data.Taxonomies?.Any() == true)
                {
                    foreach (var taxonomy in model.Data.Taxonomies ?? Enumerable.Empty<Taxonomy>())
                    {
                        dependencies.Add(CacheHelpers.GetTaxonomyDependencyKey(taxonomy.Codename));
                    }

                    dependencies.Add(CacheHelpers.GetTaxonomiesDependencyKey());
                    dependencies.Add(CacheHelpers.GetItemsDependencyKey());
                    dependencies.Add(CacheHelpers.GetTypesDependencyKey());
                }

                if (model.Message.Type == "content_type")
                {
                    dependencies.Add(CacheHelpers.GetTypesDependencyKey());
                }

                foreach (var dependency in dependencies)
                {
                    await _cacheManager.InvalidateDependencyAsync(dependency);
                }
            }

            return Ok();
        }
    }
}

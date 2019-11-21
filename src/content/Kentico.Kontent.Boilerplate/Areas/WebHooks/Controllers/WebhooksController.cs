using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Kentico.Kontent.Boilerplate.Areas.WebHooks.Models;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Boilerplate.Caching.Webhooks;

namespace Kentico.Kontent.Boilerplate.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class WebhooksController : Controller
    {
        private readonly ICacheManager _cacheManager;

        public WebhooksController(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        [HttpPost]
        public IActionResult Index([FromBody] WebhookModel model)
        {
            if (model != null)
            {
                var dependencies = new HashSet<string>();
                if (model.Data.Items?.Any() == true)
                {
                    foreach (var item in model.Data.Items ?? Enumerable.Empty<Item>())
                    {
                        dependencies.Add(CacheHelper.GetItemDependencyKey(item.Codename));
                    }

                    dependencies.Add(CacheHelper.GetItemsDependencyKey());
                }

                if (model.Data.Taxonomies?.Any() == true)
                {
                    foreach (var taxonomy in model.Data.Taxonomies ?? Enumerable.Empty<Taxonomy>())
                    {
                        dependencies.Add(CacheHelper.GetTaxonomyDependencyKey(taxonomy.Codename));
                    }

                    dependencies.Add(CacheHelper.GetTaxonomiesDependencyKey());
                    dependencies.Add(CacheHelper.GetItemsDependencyKey());
                    dependencies.Add(CacheHelper.GetTypesDependencyKey());
                }

                if (model.Message.Type == "content_type")
                {
                    dependencies.Add(CacheHelper.GetTypesDependencyKey());
                }

                foreach (var dependency in dependencies)
                {
                    _cacheManager.InvalidateDependency(dependency);
                }
            }

            return Ok();
        }
    }
}

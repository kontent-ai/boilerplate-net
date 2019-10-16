using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Kentico.Kontent.Boilerplate.Filters;
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
            _cacheManager = cacheManager;
        }

        [HttpPost]
        [ServiceFilter(typeof(SignatureActionFilter))]
        public IActionResult Index([FromBody] WebhookModel model)
        {
            if (model != null)
            {
                foreach (var item in model.Data.Items ?? Enumerable.Empty<Item>())
                {
                    _cacheManager.InvalidateDependency(CacheHelper.GetItemDependencyKey(item.Codename));
                }

                foreach (var taxonomy in model.Data.Taxonomies ?? Enumerable.Empty<Taxonomy>())
                {
                    _cacheManager.InvalidateDependency(CacheHelper.GetTaxonomyDependencyKey(taxonomy.Codename));
                }

                _cacheManager.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
                _cacheManager.InvalidateDependency(CacheHelper.GetTaxonomiesDependencyKey());

                if (model.Message.Type == "content_type")
                {
                    _cacheManager.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
                }
            }

            return Ok();
        }
    }
}

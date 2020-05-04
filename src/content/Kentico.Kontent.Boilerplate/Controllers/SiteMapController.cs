using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Delivery;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SimpleMvcSitemap;
using Microsoft.Extensions.Logging;
using Kentico.Kontent.Delivery.Abstractions;

namespace Kentico.Kontent.Boilerplate.Controllers
{
    public class SiteMapController : BaseController<SiteMapController>
    {
        public SiteMapController(IDeliveryClient deliveryClient, ILogger<SiteMapController> logger) : base(deliveryClient, logger)
        {
        }

        public async Task<ActionResult> Index()
        {
            // TODO: The different system types which should be included in the sitemap should be specified in the InFilter params
            var parameters = new List<IQueryParameter>
            {
                new DepthParameter(0),
                new InFilter("system.type", "article", "cafe"),
            };

            var response = await DeliveryClient.GetItemsAsync(parameters);

            var nodes = response.Items.Select(item => new SitemapNode(GetPageUrl(item.System))
            {
                LastModificationDate = item.System.LastModified
            })
                .ToList();

            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

        private static string GetPageUrl(ContentItemSystemAttributes system)
        {
            // TODO: The URL generation logic should be adjusted to match your website
            var url = string.Empty;

            if (system.SitemapLocation.Any())
            {
                url = $"/{system.SitemapLocation[0]}";
            }

            url = $"{url}/{system.Codename.Replace("_", "-").TrimEnd('-')}";

            return url;
        }
    }
}

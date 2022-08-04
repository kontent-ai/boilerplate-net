using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Kontent.Ai.Boilerplate.Models;
using SimpleMvcSitemap;
using Microsoft.Extensions.Logging;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Urls.Delivery.QueryParameters;
using Kontent.Ai.Urls.Delivery.QueryParameters.Filters;

namespace Kontent.Ai.Boilerplate.Controllers
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

            var response = await DeliveryClient.GetItemsAsync<object>(parameters);

            var nodes = response.Items.Cast<ISitemapItem>().Select(item => new SitemapNode(GetPageUrl(item.System))
            {
                LastModificationDate = item.System.LastModified
            }).ToList();

            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

        private static string GetPageUrl(IContentItemSystemAttributes system)
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

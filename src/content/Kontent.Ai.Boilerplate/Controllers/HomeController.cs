using System.Threading.Tasks;
using Kontent.Ai.Boilerplate.Models.ContentTypes;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Urls.Delivery.QueryParameters;
 using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kontent.Ai.Boilerplate.Controllers
{
    public class HomeController : BaseController<HomeController>
    {
        public HomeController(IDeliveryClient deliveryClient, ILogger<HomeController> logger) : base(deliveryClient, logger)
        {

        }

        public async Task<ViewResult> Index()
        {
            var response = await DeliveryClient.GetItemsAsync<Article>(
                new LimitParameter(3),
                new DepthParameter(0),
                new OrderParameter("elements.post_date")
            );

            return View(response.Items);
        }
    }
}

using System.Threading.Tasks;

using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Kontent.Boilerplate.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IDeliveryClient deliveryClient) : base(deliveryClient)
        {
            
        }

        public async Task<ViewResult> Index()
        {
            var response = await DeliveryClient.GetItemsAsync<Article>(
                new EqualsFilter("system.type", "article"),
                new LimitParameter(3),
                new DepthParameter(0),
                new OrderParameter("elements.post_date")
            );

            return View(response.Items);
        }
    }
}

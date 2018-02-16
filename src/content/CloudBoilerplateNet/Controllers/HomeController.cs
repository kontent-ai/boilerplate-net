using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IDeliveryClient deliveryClient) : base(deliveryClient)
        {
            
        }

        public async Task<ViewResult> Index()
        {
            //var response = await DeliveryClient.GetItemsAsync<Article>(
            //    new EqualsFilter("system.type", "article"),
            //    new LimitParameter(3),
            //    new DepthParameter(0),
            //    new OrderParameter("elements.post_date"),
            //    new OrderParameter("system.codename")
            //);

            //return View(response.Items);

            //var response = await DeliveryClient.GetItemsJsonAsync("system.codename=which_brewing_fits_you_", "limit=1");
            var response = await DeliveryClient.GetTaxonomiesAsync(new LimitParameter(2));
            return View();
        }
    }
}

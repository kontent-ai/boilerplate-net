using CloudBoilerplateNet.Interfaces;
using KenticoCloud.Delivery;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly DeliveryClient _client;

        public HomeController(IDeliveryClientService deliveryClientService)
        {
            _client = deliveryClientService.Client;
        }

        public async Task<ViewResult> Index()
        {
            var response = await _client.GetItemsAsync<Article>(
                new EqualsFilter("system.type", "article"),
                new LimitParameter(3)
            );

            return View(response.Items);
        }
    }
}

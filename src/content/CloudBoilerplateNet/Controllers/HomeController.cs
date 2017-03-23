using CloudBoilerplateNet.Interfaces;
using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDeliveryClient _client;

        public HomeController(IDeliveryClientService deliveryClientService)
        {
            _client = deliveryClientService.Client;
        }

        public async Task<ViewResult> Index()
        {
            var response = await _client.GetItemsAsync<Article>(
                new EqualsFilter("system.type", "article"),
                new LimitParameter(3),
                new DepthParameter(0),
                new OrderParameter("elements.post_date", SortOrder.Ascending)
            );

            return View(response.Items);
        }
    }
}

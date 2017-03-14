using CloudBoilerplateNet.Models;
using KenticoCloud.Delivery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly DeliveryClient _client;

        public HomeController(IOptions<ProjectOptions> projectOptions)
        {
            _client = new DeliveryClient(projectOptions.Value.KenticoCloudProjectId);
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

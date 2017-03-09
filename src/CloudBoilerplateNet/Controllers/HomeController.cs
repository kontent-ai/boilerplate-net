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
            var response = await _client.GetItemsAsync();

            return View(response.Items);
        }

        public IActionResult Error()
        {
            return View("~/Views/Error/404.cshtml");
        }
    }
}

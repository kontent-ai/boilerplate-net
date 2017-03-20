using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBoilerplateNet.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error/{statusCode}")]
        public ViewResult Status(int statusCode)
        {
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return View("~/Views/Error/NotFound.cshtml");
            }
            else
            {
                return View("~/Views/Error/GeneralError.cshtml");
            }
        }
    }
}

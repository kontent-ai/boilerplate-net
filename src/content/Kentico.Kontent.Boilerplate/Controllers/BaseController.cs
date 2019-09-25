using Kentico.Kontent.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Kontent.Boilerplate.Controllers
{
    public class BaseController : Controller
    {
        public BaseController(IDeliveryClient deliveryClient)
        {
            DeliveryClient = deliveryClient;
        }

        protected IDeliveryClient DeliveryClient { get; }
    }
}

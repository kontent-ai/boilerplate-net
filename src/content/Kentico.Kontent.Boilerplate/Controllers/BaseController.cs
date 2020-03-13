using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kentico.Kontent.Boilerplate.Controllers
{
    public class BaseController<T> : Controller
    {
        protected ILogger<T> Logger { get; }

        protected IDeliveryClient DeliveryClient { get; }

        public BaseController(IDeliveryClient deliveryClient, ILogger<T> logger)
        {
            DeliveryClient = deliveryClient;
            Logger = logger;
        }
    }
}

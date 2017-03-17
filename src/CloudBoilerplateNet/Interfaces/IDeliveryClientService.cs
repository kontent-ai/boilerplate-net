using KenticoCloud.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Interfaces
{
    public interface IDeliveryClientService
    {
        DeliveryClient Client { get; }
    }
}

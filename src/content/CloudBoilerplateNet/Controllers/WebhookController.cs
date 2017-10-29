using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudBoilerplateNet.Models.ContentTypes;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using KenticoCloud.Delivery;
using System.Net;

namespace CloudBoilerplateNet.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : BaseController
    {
        private readonly string kenticoCloudWebhookSecret;


        public WebhookController(IOptions<ProjectOptions> projectOptions, IDeliveryClient deliveryClient) : base(deliveryClient)
        {
            kenticoCloudWebhookSecret = projectOptions.Value.WebhookSecret;
        }

        [HttpPost]
        public async Task<IActionResult> Post(HttpRequestMessage req)
        {
            IEnumerable<string> headerValues = req.Headers.GetValues("X-KC-Signature");
            var sig = headerValues.FirstOrDefault();

            var content = req.Content;
            string jsonContent = content.ReadAsStringAsync().Result;

            var hash = GenerateHash(jsonContent);

            if (sig != hash)
            {
                return Unauthorized();
            }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                KenticoCloudWebhookModel data = JsonConvert.DeserializeObject<KenticoCloudWebhookModel>(jsonContent, settings);
                Item[] items = data.Data.Items;
                string codename = items[0].Codename;

                DeliveryItemResponse result = await DeliveryClient.GetItemAsync(codename);

                if (result == null)
                    return NoContent();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            return Accepted();
        }

        private string GenerateHash(string message)
        {
            UTF8Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            byte[] keyBytes = SafeUTF8.GetBytes(kenticoCloudWebhookSecret);
            byte[] messageBytes = SafeUTF8.GetBytes(message);
            using (HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace CloudBoilerplateNet.Filters
{
    public class KenticoCloudSignatureActionFilter : ActionFilterAttribute
    {
        private readonly string _secret;

        public KenticoCloudSignatureActionFilter(IOptions<ProjectOptions> projectOptions) => _secret = projectOptions.Value.KenticoCloudWebhookSecret;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var signature = context.HttpContext.Request.Headers["X-KC-Signature"].FirstOrDefault();
            var request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                request.Body.Position = 0;
                var content = reader.ReadToEnd();
                request.Body.Position = 0;
                var generatedSignature = GenerateHash(content, _secret);

                if (generatedSignature != signature)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }

        private static string GenerateHash(string message, string secret)
        {
            secret = secret ?? "";
            var safeUTF8 = new UTF8Encoding(false, true);
            byte[] keyBytes = safeUTF8.GetBytes(secret);
            byte[] messageBytes = safeUTF8.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);

                return Convert.ToBase64String(hashMessage);
            }
        }
    }
}
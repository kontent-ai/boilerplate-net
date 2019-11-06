using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.Boilerplate.Filters
{
    public class SignatureActionFilter : ActionFilterAttribute
    {
        private readonly string _secret;

        public SignatureActionFilter(IOptions<ProjectOptions> projectOptions) => _secret = projectOptions.Value.KenticoKontentWebhookSecret;

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
            var safeUtf8 = new UTF8Encoding(false, true);
            var keyBytes = safeUtf8.GetBytes(secret);
            var messageBytes = safeUtf8.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                var hashMessage = hmacsha256.ComputeHash(messageBytes);

                return Convert.ToBase64String(hashMessage);
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.Boilerplate.Middleware
{
    public class SignatureMiddleware
    {
        private readonly RequestDelegate _next;

        public SignatureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IOptions<ProjectOptions> projectOptions)
        {
            var request = httpContext.Request;
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
            var content = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);

            var generatedSignature = GenerateHash(content, projectOptions.Value.KenticoKontentWebhookSecret);
            var signature = request.Headers["X-KC-Signature"].FirstOrDefault();

            if (generatedSignature != signature)
            {
                httpContext.Response.StatusCode = 401;
                return;
            }

            await _next(httpContext);
        }

        private static string GenerateHash(string message, string secret)
        {
            secret ??= "";
            var safeUtf8 = new UTF8Encoding(false, true);
            var keyBytes = safeUtf8.GetBytes(secret);
            var messageBytes = safeUtf8.GetBytes(message);

            using var hmacsha256 = new HMACSHA256(keyBytes);
            var hashMessage = hmacsha256.ComputeHash(messageBytes);

            return Convert.ToBase64String(hashMessage);
        }
    }
}
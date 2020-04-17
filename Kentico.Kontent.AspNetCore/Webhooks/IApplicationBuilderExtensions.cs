using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Kentico.Kontent.AspNetCore.Middleware.Webhook
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebhookSignatureValidator(this IApplicationBuilder app, Func<HttpContext, bool> predicate, WebhookOptions options = null)
        {
            app.UseWhen(predicate, appBuilder =>
            {
                if (options != null)
                {
                    appBuilder.UseMiddleware<SignatureMiddleware>((IOptions<WebhookOptions>)Options.Create(options));
                }
                else
                {
                    appBuilder.UseMiddleware<SignatureMiddleware>();
                }
            });

            return app;
        }

        public static IApplicationBuilder UseWebhookSignatureValidator(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<WebhookOptions> configureOptions)
        {
            var options = new WebhookOptions();
            configureOptions(options);

            return app.UseWebhookSignatureValidator(predicate, options);
        }

        public static IApplicationBuilder UseWebhookSignatureValidator(this IApplicationBuilder app, Func<HttpContext, bool> predicate, IConfigurationSection configurationSection)
        {
            var options = new WebhookOptions();
            configurationSection.Bind(options);

            return app.UseWebhookSignatureValidator(predicate, options);
        }
    }
}

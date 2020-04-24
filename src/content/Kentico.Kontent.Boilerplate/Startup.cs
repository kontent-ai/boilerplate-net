using System;
using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Boilerplate.Resolvers;
using Kentico.Kontent.Delivery.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kentico.Kontent.Delivery.Caching.Extensions;
using Kentico.Kontent.AspNetCore.Middleware.Webhook;
using Kentico.Kontent.AspNetCore.ImageTransformation;

namespace Kentico.Kontent.Boilerplate
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds services required for using options.
            services.AddOptions();

            // Register the ImageTransformationOptions required by Kentico Kontent tag helpers
            services.Configure<ImageTransformationOptions>(Configuration.GetSection(nameof(ImageTransformationOptions)));

            services.AddSingleton<CustomTypeProvider>();
            services.AddSingleton<CustomContentLinkUrlResolver>();
            services.AddDeliveryClient(Configuration);

            // Use cached client decorator
            services.AddDeliveryClientCache(new DeliveryCacheOptions()
            {
                StaleContentExpiration = TimeSpan.FromSeconds(2),
                DefaultExpiration = TimeSpan.FromMinutes(24)
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Add IIS URL Rewrite list, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting
            var options = new RewriteOptions().AddIISUrlRewrite(env.ContentRootFileProvider, "IISUrlRewrite.xml");
            app.UseRewriter(options);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();

            // Register webhook-based cache invalidation controller
            app.UseWebhookSignatureValidator(context => context.Request.Path.StartsWithSegments("/webhooks/webhooks", StringComparison.OrdinalIgnoreCase), Configuration.GetSection(nameof(WebhookOptions)));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "sitemap",
                    defaults: new { controller = "Sitemap", action = "Index" },
                    pattern: "sitemap.xml");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

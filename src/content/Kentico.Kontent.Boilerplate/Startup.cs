using System;
using Kentico.Kontent.Boilerplate.Helpers.Extensions;
using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Boilerplate.Resolvers;
using Kentico.Kontent.Delivery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Boilerplate.Middleware;

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

            // Register the IConfiguration instance which ProjectOptions binds against.
            services.Configure<ProjectOptions>(Configuration);

            var deliveryOptions = new DeliveryOptions();
            Configuration.GetSection(nameof(DeliveryOptions)).Bind(deliveryOptions);

            IDeliveryClient BuildBaseClient(IServiceProvider sp) => DeliveryClientBuilder
                .WithOptions(_ => deliveryOptions)
                .WithTypeProvider(new CustomTypeProvider())
                .WithContentLinkUrlResolver(new CustomContentLinkUrlResolver())
                .Build();

            // Use cached client version based on the use case
            services.AddCachingClient(BuildBaseClient, options =>
            {
                options.StaleContentExpiration = TimeSpan.FromSeconds(2);
                options.DefaultExpiration = TimeSpan.FromMinutes(10);
            });
            //services.AddWebhookInvalidatedCachingClient(BuildBaseClient, options =>
            //{
            //    options.StaleContentExpiration = TimeSpan.FromSeconds(2);
            //    options.DefaultExpiration = TimeSpan.FromHours(24);
            //});

            HtmlHelperExtensions.ProjectOptions = Configuration.Get<ProjectOptions>();

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

            // Add IIS URL Rewrite list
            // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting
            var options = new RewriteOptions().AddIISUrlRewrite(env.ContentRootFileProvider, "IISUrlRewrite.xml");
            app.UseRewriter(options);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/webhooks/webhooks", StringComparison.OrdinalIgnoreCase), appBuilder =>
            {
                appBuilder.UseMiddleware<SignatureMiddleware>();
            });

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

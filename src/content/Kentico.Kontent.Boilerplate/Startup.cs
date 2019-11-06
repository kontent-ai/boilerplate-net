using System;
using Kentico.Kontent.Boilerplate.Filters;
using Kentico.Kontent.Boilerplate.Helpers.Extensions;
using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Boilerplate.Resolvers;
using Kentico.Kontent.Delivery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Kentico.Kontent.Boilerplate.Caching;

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

            services.AddScoped<SignatureActionFilter>();

            IDeliveryClient BuildBaseClient(IServiceProvider sp) => DeliveryClientBuilder
                .WithOptions(_ => deliveryOptions)
                .WithTypeProvider(new CustomTypeProvider())
                .WithContentLinkUrlResolver(new CustomContentLinkUrlResolver())
                .Build();

            // Use cached client version based on the use case
            //services.AddCachingClient(BuildBaseClient, options =>
            //{
            //    options.StaleContentTimeout = TimeSpan.FromSeconds(2);
            //    options.DefaultTimeout = TimeSpan.FromSeconds(20);
            //});
            services.AddWebhookInvalidatedCachingClient(BuildBaseClient, options =>
            {
                options.StaleContentTimeout = TimeSpan.FromSeconds(2);
                options.DefaultTimeout = TimeSpan.FromHours(24);
            });

            HtmlHelperExtensions.ProjectOptions = services.BuildServiceProvider().GetService<IOptions<ProjectOptions>>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Home/Error");
            }

            // Add IIS URL Rewrite list
            // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting
            using (StreamReader iisUrlRewriteStreamReader = File.OpenText("IISUrlRewrite.xml"))
            {
                var options = new RewriteOptions()
                    .AddIISUrlRewrite(iisUrlRewriteStreamReader);

                app.UseRewriter(options);
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "sitemap",
                    defaults: new { controller = "Sitemap", action = "Index" },
                    template: "sitemap.xml");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

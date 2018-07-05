using System;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;

using CloudBoilerplateNet.Filters;
using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Helpers.Extensions;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Resolvers;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CloudBoilerplateNet
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

            services.AddSingleton<IWebhookListener>(sp => new WebhookListener());
            services.AddSingleton<IDependentTypesResolver>(sp => new DependentFormatResolver());
            services.AddSingleton<ICacheManager>(sp => new ReactiveCacheManager(
                sp.GetRequiredService<IOptions<ProjectOptions>>(), 
                sp.GetRequiredService<IMemoryCache>(), 
                sp.GetRequiredService<IDependentTypesResolver>(), 
                sp.GetRequiredService<IWebhookListener>()));

            services.AddScoped<KenticoCloudSignatureActionFilter>();

            services.AddSingleton<IDeliveryClient>(sp => new CachedDeliveryClient(
                sp.GetRequiredService<IOptions<ProjectOptions>>(), 
                sp.GetRequiredService<ICacheManager>())
            {
                CodeFirstModelProvider = { TypeProvider = new CustomTypeProvider() },
                ContentLinkUrlResolver = new CustomContentLinkUrlResolver()
            });

            HtmlHelperExtensions.ProjectOptions = services.BuildServiceProvider().GetService<IOptions<ProjectOptions>>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
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

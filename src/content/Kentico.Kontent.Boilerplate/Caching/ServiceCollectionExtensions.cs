using System;
using Kentico.Kontent.Boilerplate.Caching.Webhooks;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Kontent.Boilerplate.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCachingClient(this IServiceCollection services, Func<IServiceProvider, IDeliveryClient> baseClientFactory, Action<CacheOptions> configureCacheOptions = null)
        {
            if (configureCacheOptions != null)
            {
                services.Configure(configureCacheOptions);
            }
            services.TryAddSingleton<Default.ICacheManager, Default.CacheManager>();

            services.AddSingleton<IDeliveryClient>(sp => new Default.CachingDeliveryClient(
                sp.GetRequiredService<Default.ICacheManager>(),
                baseClientFactory(sp)));
            return services;
        }

        public static IServiceCollection AddWebhookInvalidatedCachingClient(this IServiceCollection services, Func<IServiceProvider, IDeliveryClient> baseClientFactory, Action<CacheOptions> configureCacheOptions = null)
        {
            if (configureCacheOptions != null)
            {
                services.Configure(configureCacheOptions);
            }
            services.TryAddSingleton<ICacheManager, CacheManager>();

            services.AddSingleton<IDeliveryClient>(sp => new CachingDeliveryClient(
                sp.GetRequiredService<ICacheManager>(),
                baseClientFactory(sp)));
            return services;
        }
    }
}
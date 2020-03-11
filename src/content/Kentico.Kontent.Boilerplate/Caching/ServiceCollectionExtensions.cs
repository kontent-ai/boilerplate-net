using System;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Kontent.Boilerplate.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCachingClient(this IServiceCollection services, Func<IServiceProvider, IDeliveryClient> baseClientFactory, Action<CacheOptions> configureCacheOptions = null)
        {
            return AddCachingClient<ExpiringCacheManager>(services, baseClientFactory, configureCacheOptions);
        }

        public static IServiceCollection AddWebhookInvalidatedCachingClient(this IServiceCollection services, Func<IServiceProvider, IDeliveryClient> baseClientFactory, Action<CacheOptions> configureCacheOptions = null)
        {
            return AddCachingClient<InvalidatingCacheManager>(services, baseClientFactory, configureCacheOptions);
        }


        public static IServiceCollection AddCachingClient<T>(this IServiceCollection services, Func<IServiceProvider, IDeliveryClient> baseClientFactory, Action<CacheOptions> configureCacheOptions = null) where T : class, ICacheManager
        {
            if (configureCacheOptions != null)
            {
                services.Configure(configureCacheOptions);
            }
            services.TryAddSingleton<ICacheManager, T>();

            services.AddSingleton<IDeliveryClient>(sp => new CachingDeliveryClient(
                sp.GetRequiredService<ICacheManager>(),
                baseClientFactory(sp)));
            return services;
        }
    }
}
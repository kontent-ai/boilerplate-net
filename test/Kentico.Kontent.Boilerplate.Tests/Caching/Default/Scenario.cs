using System.Collections.Generic;
using System.Net.Http;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.Boilerplate.Tests.Caching.Default
{
    public class Scenario
    {
        private readonly Dictionary<string, int> _requestCounter;
        public CachingDeliveryClient CachingClient { get; }

        public Scenario(IMemoryCache memoryCache, HttpClient httpClient, DeliveryOptions deliveryOptions, Dictionary<string, int> requestCounter)
        {
            _requestCounter = requestCounter;
            var cacheManager = new ExpiringCacheManager(memoryCache, Options.Create(new CacheOptions()));
            var baseClient = DeliveryClientBuilder.WithOptions(_ => deliveryOptions).WithHttpClient(httpClient).Build();
            CachingClient = new CachingDeliveryClient(cacheManager, baseClient);
        }

        public int GetRequestCount(string url) => _requestCounter.GetValueOrDefault(url);
    }
}
using System;

namespace Kentico.Kontent.Boilerplate.Caching
{
    public class CacheOptions
    {
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan StaleContentExpiration { get; set; } = TimeSpan.FromSeconds(10);
    }
}
using System;

namespace Kentico.Kontent.Boilerplate.Caching
{
    public class CacheOptions
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan StaleContentTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
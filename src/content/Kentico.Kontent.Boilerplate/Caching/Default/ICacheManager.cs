using System;
using System.Threading.Tasks;

namespace Kentico.Kontent.Boilerplate.Caching.Default
{
    public interface ICacheManager
    {
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, Func<T, bool> shouldCache = null);
        bool TryGet<T>(string key, out T value);
    }
}
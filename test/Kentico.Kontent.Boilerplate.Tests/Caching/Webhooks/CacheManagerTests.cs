using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Boilerplate.Caching.Webhooks;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NSubstitute;
using RichardSzalay.MockHttp;
using Xunit;

namespace Kentico.Kontent.Boilerplate.Tests.Caching.Webhooks
{
    public class CacheManagerTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheManager _cacheManager;
        private readonly CacheOptions _cacheOptions;

        public CacheManagerTests()
        {
            // Create memory cache with spy
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            _memoryCache = Substitute.For<IMemoryCache>();
            _memoryCache.TryGetValue(Arg.Any<object>(), out Arg.Any<object>()).Returns(c =>
            {
                var result = memoryCache.TryGetValue(c[0], out var value);
                c[1] = value;
                return result;
            });
            _memoryCache.CreateEntry(Arg.Any<object>()).Returns(c => memoryCache.CreateEntry(c[0]));

            _cacheOptions = new CacheOptions();
            _cacheManager = new CacheManager(_memoryCache, Options.Create(_cacheOptions));
        }

        [Fact]
        public async Task GetOrAddAsync_ValueIsCached_ReturnsCachedValue()
        {
            const string key = "key";
            var originalValue = _memoryCache.Set(key, "value");

            var result = await _cacheManager.GetOrAddAsync<string>(key, null, null);

            result.Should().Be(originalValue);
        }

        [Fact]
        public async Task GetOrAddAsync_ValueShouldNotBeCached_DoesNotCacheNewValue()
        {
            const string key = "key";
            const string value = "newValue";

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), null, _ => false);

            result.Should().Be(value);
            _memoryCache.TryGetValue(key, out _).Should().BeFalse();
        }

        [Fact]
        public async Task GetOrAddAsync_ValueIsNotCached_CachesNewValue()
        {
            const string key = "key";
            const string value = "newValue";

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), null);

            result.Should().Be(value);
            _memoryCache.TryGetValue(key, out string cachedValue).Should().BeTrue();
            cachedValue.Should().Be(value);
        }

        [Fact]
        public async Task GetOrAddAsync_ValueIsNotCached_DependencyIsNotCached_CachesNewDependencies()
        {
            const string key = "key";
            const string value = "newValue";
            var dependencies = new[]
            {
                "dependency_1",
                "dependency_2"
            };

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), _ => dependencies);

            result.Should().Be(value);
            _memoryCache.TryGetValue(key, out string cachedValue).Should().BeTrue();
            cachedValue.Should().Be(value);
            dependencies.Select(x => _memoryCache.TryGetValue(x, out CancellationTokenSource ts) && !ts.IsCancellationRequested)
                .Should().OnlyContain(x => x);
        }

        [Fact]
        public async Task GetOrAddAsync_ValueIsNotCached_DependencyIsCached_DoesNotCacheDependency()
        {
            const string key = "key";
            const string value = "newValue";
            var dependency = "dependency_1";
            var cachedDependencyValue = _memoryCache.Set(dependency, new CancellationTokenSource());

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), _ => new[] { dependency });

            Assert.Equal(value, result);
            Assert.True(_memoryCache.TryGetValue(key, out string cachedValue));
            Assert.Equal(value, cachedValue);
            Assert.True(_memoryCache.TryGetValue(dependency, out CancellationTokenSource tokenSource) && !tokenSource.IsCancellationRequested);
            Assert.Equal(cachedDependencyValue, tokenSource);
        }

        [Fact]
        public async Task GetOrAddAsync_ValueIsNotCached_DependencyIsExpired_CachesNewDependency()
        {
            const string key = "key";
            const string value = "newValue";
            var dependency = "dependency_1";
            var cachedDependencyValue = _memoryCache.Set(dependency, new CancellationTokenSource());
            cachedDependencyValue.Cancel();

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), _ => new[] { dependency });

            Assert.Equal(value, result);
            Assert.True(_memoryCache.TryGetValue(key, out string cachedValue));
            Assert.Equal(value, cachedValue);
            Assert.True(_memoryCache.TryGetValue(dependency, out CancellationTokenSource tokenSource) && !tokenSource.IsCancellationRequested);
            Assert.NotEqual(cachedDependencyValue, tokenSource);
        }

        [Fact]
        public async Task GetOrAddAsync_CachedValueExpiresWhenDependencyInvalidated()
        {
            const string key = "key";
            const string value = "newValue";
            const string dependency = "dependency_1";

            var result = await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), _ => new[] { dependency });
            _cacheManager.InvalidateDependency(dependency);

            Assert.Equal(value, result);
            Assert.False(_memoryCache.TryGetValue(key, out _));
            Assert.True(_memoryCache.TryGetValue(dependency, out CancellationTokenSource tokenSource) && tokenSource.IsCancellationRequested);
        }

        [Fact]
        public async Task GetOrAddAsync_IsNotStaleContent_CachedValueExpiresAfterDefaultTimeout()
        {
            const string key = "key";
            var value = await GetAbstractResponseInstance(false);
            _cacheOptions.DefaultExpiration = TimeSpan.FromMilliseconds(500);

            await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), null);
            await Task.Delay(_cacheOptions.DefaultExpiration);

            Assert.False(_memoryCache.TryGetValue(key, out _));
        }

        [Fact]
        public async Task GetOrAddAsync_IsStaleContent_CachedValueExpiresAfterStaleContentTimeout()
        {
            const string key = "key";
            var value = await GetAbstractResponseInstance(true);
            _cacheOptions.StaleContentExpiration = TimeSpan.FromMilliseconds(500);

            await _cacheManager.GetOrAddAsync(key, () => Task.FromResult(value), null);
            await Task.Delay(_cacheOptions.StaleContentExpiration);

            Assert.False(_memoryCache.TryGetValue(key, out _));
        }

        [Fact]
        public async Task GetOrAddAsync_ParallelAccess_ValueFetchedAndCachedOnlyOnce()
        {
            const string key = "key;";
            var counter = 0;
            Task<int> WaitAndIncreaseCounter() => Task.Delay(200).ContinueWith(_ => Interlocked.Increment(ref counter));

            var tasks = Enumerable.Range(0, 100).Select(i => _cacheManager.GetOrAddAsync(key, WaitAndIncreaseCounter, null));
            var results = await Task.WhenAll(tasks);

            Assert.All(results, v => Assert.Equal(1, v));
            _memoryCache.Received(1).CreateEntry(Arg.Is(key));

        }

        [Fact]
        public async Task GetOrAddAsync_ParallelAccess_DependenciesCachedOnlyOnce()
        {
            const string dependencyKey = "dependency_key";
            var dependencies = Enumerable.Repeat(dependencyKey, 100);
            // Add delay to force parallel processing
            Task<object> WaitAndGetValue(object v) => Task.Delay(100).ContinueWith(_ => v);

            var tasks = Enumerable.Range(0, 100)
                .Select(i => i.ToString())
                .Select(i => _cacheManager.GetOrAddAsync(i, () => WaitAndGetValue(i), _ => dependencies));
            await Task.WhenAll(tasks);

            _memoryCache.Received(1).CreateEntry(Arg.Is(dependencyKey));
        }

        [Fact]
        public void TryGet_ValueIsNotCached_ReturnsFalse()
        {
            const string key = "key";

            var result = _cacheManager.TryGet<string>(key, out _);

            Assert.False(result);
        }

        [Fact]
        public void TryGet_ValueIsCached_ReturnsTrueAndCachedValue()
        {
            const string key = "key";
            var originalValue = _memoryCache.Set(key, "cachedString");

            var result = _cacheManager.TryGet<string>(key, out var cachedValue);

            Assert.True(result);
            Assert.Equal(originalValue, cachedValue);
        }

        [Fact]
        public void TryGet_KeyIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _cacheManager.TryGet<string>(null, out _));
        }

        [Fact]
        public void TryGet_ValueIsExpired_ReturnsFalse()
        {
            const string key = "key";
            var tokenSource = new CancellationTokenSource();
            var cancelToken = new CancellationChangeToken(tokenSource.Token);
            _memoryCache.Set(key, "cachedString", cancelToken);
            tokenSource.Cancel();

            var result = _cacheManager.TryGet<string>(key, out _);

            Assert.False(result);
        }

        [Fact]
        public void InvalidateDependency_KeyIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _cacheManager.InvalidateDependency(null));
        }

        [Fact]
        public void InvalidateDependency_DependencyTokenExists_CancellationRequested()
        {
            const string key = "key";
            var value = _memoryCache.Set(key, new CancellationTokenSource());

            _cacheManager.InvalidateDependency(key);

            Assert.True(value.IsCancellationRequested);
        }

        private static async Task<AbstractResponse> GetAbstractResponseInstance(bool shouldBeStaleContent)
        {
            var itemsResponse = new
            {
                items = Enumerable.Empty<object>(),
                modular_content = new Dictionary<string, object>()
            };

            var staleContentHeaderValue = shouldBeStaleContent ? "1" : "0";
            var mockHandler = new MockHttpMessageHandler();
            var responseHeaders = new[] { new KeyValuePair<string, string>("X-Stale-Content", staleContentHeaderValue) };
            mockHandler.When("*").Respond(responseHeaders, "application/json", JsonConvert.SerializeObject(itemsResponse));
            var httpClient = mockHandler.ToHttpClient();
            var client = DeliveryClientBuilder.WithProjectId(Guid.NewGuid()).WithHttpClient(httpClient).Build();
            return await client.GetItemsAsync();
        }
    }
}
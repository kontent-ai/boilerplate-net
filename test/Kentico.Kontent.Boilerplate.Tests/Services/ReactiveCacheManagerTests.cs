using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kentico.Kontent.Boilerplate.Helpers;
using Kentico.Kontent.Boilerplate.Models;
using Kentico.Kontent.Boilerplate.Resolvers;
using Kentico.Kontent.Boilerplate.Services;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kentico.Kontent.Boilerplate.Tests.Services
{
    public class ReactiveCacheManagerTests
    {
        private const string TEST_VALUE = "TestValue";

        [Fact]
        public void CreatesEntry()
        {
            PrepareFixture(out ReactiveCacheManager cacheManager, out List<string> identifiers, out string value);
            cacheManager.CreateEntry(identifiers, value, ItemFormatDependencyFactory);

            Assert.Equal(value, cacheManager.MemoryCache.Get<string>(identifiers.First()));
            Assert.NotNull(cacheManager.MemoryCache.Get<CancellationTokenSource>(
                    string.Join("|", "dummy", ItemFormatDependencyFactory(value).First().Type, ItemFormatDependencyFactory(value).First().Codename)
                    ));
        }

        [Fact]
        public async Task CreatesEntryIfNotExists()
        {
            PrepareFixture(out ReactiveCacheManager cacheManager, out List<string> identifiers, out string value);
            var cacheEntry = await cacheManager.GetOrCreateAsync(identifiers, ValueFactory, (response) => false, ItemFormatDependencyFactory, false);

            Assert.Equal(value, cacheManager.MemoryCache.Get<string>(identifiers.First()));
            Assert.NotNull(cacheManager.MemoryCache.Get<CancellationTokenSource>(
                    string.Join("|", "dummy", ItemFormatDependencyFactory(value).First().Type, ItemFormatDependencyFactory(value).First().Codename)
                    ));
        }

        [Fact]
        public void InvalidatesEntry()
        {
            PrepareFixture(out ReactiveCacheManager cacheManager, out List<string> identifiers, out string value);
            cacheManager.CreateEntry(identifiers, value, ItemFormatDependencyFactory);
            cacheManager.InvalidateEntry(ItemFormatDependencyFactory(value).First());

            Assert.Null(cacheManager.MemoryCache.Get<string>(identifiers.First()));
        }

        [Fact]
        public void InvalidatesDependentTypes()
        {
            var cacheManager = BuildCacheManager();
            cacheManager.CreateEntry(new List<string> { "TestItem" }, "TestItemValue", ItemFormatDependencyFactory);
            cacheManager.CreateEntry(new List<string> { "TestVariant" }, "TestVariantValue", ItemFormatDependencyFactory);
            cacheManager.InvalidateEntry(ItemFormatDependencyFactory(null).ElementAt(0));

            Assert.Null(cacheManager.MemoryCache.Get<string>("TestItem"));
            Assert.Null(cacheManager.MemoryCache.Get<string>("TestVariant"));
        }

        private void PrepareFixture(out ReactiveCacheManager cacheManager, out List<string> identifier, out string value)
        {
            cacheManager = BuildCacheManager();
            identifier = new List<string> { "TestIdentifier" };
            value = "TestValue";
        }

        private Task<string> ValueFactory()
        {
            return Task.FromResult(TEST_VALUE);
        }

        private List<IdentifierSet> ItemFormatDependencyFactory(string value)
        {
            return new List<IdentifierSet>
            {
                new IdentifierSet
                {
                    Type = CacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                    Codename = "TestItem"
                }
            };
        }

        private ReactiveCacheManager BuildCacheManager()
        {
            var projectOptions = Options.Create(new ProjectOptions
            {
                CacheTimeoutSeconds = 60,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = Guid.NewGuid().ToString()
                }
            });

            var memoryCacheOptions = Options.Create(new MemoryCacheOptions
            {
                Clock = new TestClock(),
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });

            return new ReactiveCacheManager(projectOptions, new MemoryCache(memoryCacheOptions), new DependentFormatResolver(), new WebhookListener());
        }
    }
}

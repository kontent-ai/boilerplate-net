using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace CloudBoilerplateNet.Tests.Services
{
    public class ReactiveCacheManagerTests
    {
        private const string TEST_VALUE = "TestValue";

        [Fact]
        public void CreatesEntry()
        {
            ReactiveCacheManager cacheManager;
            List<string> identifiers;
            string value;
            PrepareFixture(out cacheManager, out identifiers, out value);
            cacheManager.CreateEntry(identifiers, value, ItemVariantDependencyFactory);

            Assert.Equal(value, cacheManager.MemoryCache.Get<string>(identifiers.First()));
            Assert.NotNull(cacheManager.MemoryCache.Get<CancellationTokenSource>(
                    string.Join("|", "dummy", ItemVariantDependencyFactory(value).First().Type, ItemVariantDependencyFactory(value).First().Codename)
                    ));
        }

        [Fact]
        public async void CreatesEntryIfNotExists()
        {
            ReactiveCacheManager cacheManager;
            List<string> identifiers;
            string value;
            PrepareFixture(out cacheManager, out identifiers, out value);
            var cacheItem = await cacheManager.GetOrCreateAsync(identifiers, ValueFactory, ItemVariantDependencyFactory);

            Assert.Equal(value, cacheManager.MemoryCache.Get<string>(identifiers.First()));
            Assert.NotNull(cacheManager.MemoryCache.Get<CancellationTokenSource>(
                    string.Join("|", "dummy", ItemVariantDependencyFactory(value).First().Type, ItemVariantDependencyFactory(value).First().Codename)
                    ));
        }

        [Fact]
        public void InvalidatesEntry()
        {
            ReactiveCacheManager cacheManager;
            List<string> identifiers;
            string value;
            PrepareFixture(out cacheManager, out identifiers, out value);
            cacheManager.CreateEntry(identifiers, value, ItemVariantDependencyFactory);
            cacheManager.InvalidateEntry(ItemVariantDependencyFactory(value).First());

            Assert.Null(cacheManager.MemoryCache.Get<string>(identifiers.First()));
        }

        [Fact]
        public void InvalidatesDependentTypes()
        {
            var cacheManager = BuildCacheManager();
            cacheManager.CreateEntry(new List<string> { "TestItem" }, "TestItemValue", ItemVariantDependencyFactory);
            cacheManager.CreateEntry(new List<string> { "TestVariant" }, "TestVariantValue", ItemVariantDependencyFactory);
            cacheManager.InvalidateEntry(ItemVariantDependencyFactory("TestVariantValue").ElementAt(1));

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

        private List<IdentifierSet> ItemVariantDependencyFactory(string value)
        {
            return new List<IdentifierSet>
                {
                    new IdentifierSet
                    {
                        Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER,
                        Codename = value
                    },

                    new IdentifierSet
                    {
                        Type = KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER,
                        Codename = value
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

            return new ReactiveCacheManager(projectOptions, new MemoryCache(memoryCacheOptions), new KenticoCloudDependentFormatResolver(), new KenticoCloudWebhookListener());
        }
    }
}

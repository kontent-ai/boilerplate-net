using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Resolvers;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace CloudBoilerplateNet.Tests
{
    public class CachedDeliveryClientTests
    {
        readonly string guid = string.Empty;
        readonly string baseUrl = string.Empty;
        readonly MockHttpMessageHandler mockHttp;

        public CachedDeliveryClientTests()
        {
            guid = Guid.NewGuid().ToString();
            baseUrl = $"https://deliver.kenticocloud.com/{guid}";
            mockHttp = new MockHttpMessageHandler();
        }

        [Fact]
        public async void GetsContentItemSingleDependencies()
        {
            var client = GetDeliveryClient(MockItem);
            var cachedClient = GetCachedDeliveryClient(MockItem);
            var response = await client.GetItemAsync("coffee_beverages_explained", new LanguageParameter("es-ES"));
            var dependencies = cachedClient.GetContentItemSingleDependencies(response);

            AssertItemSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentItemSingleJsonDependencies()
        {
            var client = GetDeliveryClient(MockItem);
            var cachedClient = GetCachedDeliveryClient(MockItem);
            var response = await client.GetItemJsonAsync("coffee_beverages_explained", "language=es-ES");
            var dependencies = cachedClient.GetContentItemSingleJsonDependencies(response);

            AssertItemSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentItemListingDependencies()
        {
            var client = GetDeliveryClient(MockItems);
            var cachedClient = GetCachedDeliveryClient(MockItems);
            var response = await client.GetItemsAsync(new LimitParameter(2), new SkipParameter(1));
            var dependencies = cachedClient.GetContentItemListingDependencies(response);

            AssertItemListingDependencies(dependencies);
        }


        [Fact]
        public async void GetsContentItemListingJsonDependencies()
        {
            var client = GetDeliveryClient(MockItems);
            var cachedClient = GetCachedDeliveryClient(MockItems);
            var response = await client.GetItemsJsonAsync("limit=2", "skip=1");
            var dependencies = cachedClient.GetContentItemListingJsonDependencies(response);

            AssertItemListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentElementDependency()
        {
            var client = GetDeliveryClient(MockElement);
            var cachedClient = GetCachedDeliveryClient(MockElement);
            var response = await client.GetContentElementAsync(Models.Article.Codename, Models.Article.TitleCodename);
            var dependencies = cachedClient.GetContentElementDependencies(response);

            Assert.Equal(2, dependencies.Count());
            Assert.All(dependencies, d => d.Codename.Equals("text|title", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("content_element", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("content_element_json", StringComparison.Ordinal));
        }

        [Fact]
        public async void GetsTaxonomySingleDependency()
        {
            var client = GetDeliveryClient(MockTaxonomy);
            var cachedClient = GetCachedDeliveryClient(MockTaxonomy);
            var response = await client.GetTaxonomyAsync("personas");
            var dependencies = cachedClient.GetTaxonomySingleDependency(response);

            AssertTaxonomySingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomySingleJsonDependency()
        {
            var client = GetDeliveryClient(MockTaxonomy);
            var cachedClient = GetCachedDeliveryClient(MockTaxonomy);
            var response = await client.GetTaxonomyJsonAsync("personas");
            var dependencies = cachedClient.GetTaxonomySingleJsonDependency(response);

            AssertTaxonomySingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomyListingDependencies()
        {
            var client = GetDeliveryClient(MockTaxonomies);
            var cachedClient = GetCachedDeliveryClient(MockTaxonomies);
            var response = await client.GetTaxonomiesAsync(new SkipParameter(1));
            var dependencies = cachedClient.GetTaxonomyListingDependencies(response);

            AssertTaxonomyListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomyListingJsonDependencies()
        {
            var client = GetDeliveryClient(MockTaxonomies);
            var cachedClient = GetCachedDeliveryClient(MockTaxonomies);
            var response = await client.GetTaxonomiesJsonAsync("skip=1");
            var dependencies = cachedClient.GetTaxonomyListingJsonDependencies(response);

            AssertTaxonomyListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeSingleDependencies()
        {
            var client = GetDeliveryClient(MockType);
            var cachedClient = GetCachedDeliveryClient(MockType);
            var response = await client.GetTypeAsync(Models.Article.Codename);
            var dependencies = cachedClient.GetTypeSingleDependencies(response);

            AssertTypeSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeSingleJsonDependencies()
        {
            var client = GetDeliveryClient(MockType);
            var cachedClient = GetCachedDeliveryClient(MockType);
            var response = await client.GetTypeJsonAsync(Models.Article.Codename);
            var dependencies = cachedClient.GetTypeSingleJsonDependencies(response);

            AssertTypeSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeListingDependencies()
        {
            var client = GetDeliveryClient(MockTypes);
            var cachedClient = GetCachedDeliveryClient(MockTypes);
            var response = await client.GetTypesAsync(new SkipParameter(2));
            var dependencies = cachedClient.GetTypeListingDependencies(response);

            AssertTypeListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeListingJsonDependencies()
        {
            var client = GetDeliveryClient(MockTypes);
            var cachedClient = GetCachedDeliveryClient(MockTypes);
            var response = await client.GetTypesJsonAsync("skip=2");
            var dependencies = cachedClient.GetTypeListingJsonDependencies(response);

            AssertTypeListingDependencies(dependencies);
        }

        [Fact]
        public async void GetEntryFromCacheAfterFirstRequest()
        {
            var plannedHttpRequests = 2;
            var actualHttpRequests = new RequestCount();
            var cachedClient = GetCachedDeliveryClient(mockFunc: MockItemAndLogHttpRequests, actualHttpRequests: actualHttpRequests);

            for (int i = 0; i < plannedHttpRequests; i++)
            {
                var response = await cachedClient.GetItemAsync("coffee_beverages_explained", new LanguageParameter("es-ES"));

                // ReactiveCacheManager.CreateEntry needs time to build the cache entry in the background thread. Otherwise actualHttpRequests might be incremented once more.
                await Task.Delay(500);
            }

            Assert.Equal(1, actualHttpRequests.Value);
        }

        [Fact]
        public async void AvoidsAddingNullIdentifierTokens()
        {
            var cachedClient = GetCachedDeliveryClient(() =>
            {
                mockHttp.When($"{baseUrl}/items/coffee_beverages_explained")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\item_with_null_identifiers.json")));
            });

            var item = (await cachedClient.GetItemAsync<Article>("coffee_beverages_explained")).Item;

            Assert.NotNull(item);
        }

        private DeliveryClient GetDeliveryClient(Action mockAction)
        {
            InitClientPrerequisites(out HttpClient httpClient, out DeliveryOptions deliveryOptions, mockAction);

            return new DeliveryClient(deliveryOptions)
            {
                CodeFirstModelProvider = { TypeProvider = new Models.CustomTypeProvider() },
                HttpClient = httpClient
            };
        }

        private CachedDeliveryClient GetCachedDeliveryClient(Action mockAction = null, Func<RequestCount, RequestCount> mockFunc = null, RequestCount actualHttpRequests = null)
        {
            HttpClient httpClient = null;
            DeliveryOptions deliveryOptions = null;

            if (mockAction != null)
            {
                InitClientPrerequisites(out httpClient, out deliveryOptions, mockAction: mockAction);
            }
            else if (mockFunc != null && actualHttpRequests != null)
            {
                InitClientPrerequisites(out httpClient, out deliveryOptions, mockFunc: mockFunc, actualHttpRequests: actualHttpRequests);
            }

            var projectOptions = Options.Create(new ProjectOptions
            {
                CacheTimeoutSeconds = 60,
                DeliveryOptions = deliveryOptions
            });

            var memoryCacheOptions = Options.Create(new MemoryCacheOptions
            {
                Clock = new TestClock(),
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });

            var cacheManager = new ReactiveCacheManager(projectOptions, new MemoryCache(memoryCacheOptions), new DependentFormatResolver(), new WebhookListener());

            return new CachedDeliveryClient(projectOptions, cacheManager
                , new DeliveryClient(deliveryOptions)
                {
                    HttpClient = httpClient
                })
            {
                CodeFirstModelProvider = { TypeProvider = new Models.CustomTypeProvider() }
            };
        }

        private void InitClientPrerequisites(out HttpClient httpClient, out DeliveryOptions deliveryOptions, Action mockAction = null, Func<RequestCount, RequestCount> mockFunc = null, RequestCount actualHttpRequests = null)
        {
            if (mockAction != null)
            {
                mockAction();
            }
            else
            {
                mockFunc?.Invoke(actualHttpRequests);
            }

            httpClient = mockHttp.ToHttpClient();

            deliveryOptions = new DeliveryOptions
            {
                ProjectId = guid
            };
        }

        private void MockItem()
        {
            mockHttp.When($"{baseUrl}/items/{"coffee_beverages_explained"}?language=es-ES")
                .Respond("application/json", GetMockedItemJsonStream());
        }

        private void MockItems()
        {
            mockHttp.When($"{baseUrl}/items")
                .WithQueryString("limit=2&skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\items.json")));
        }

        private void MockType()
        {
            mockHttp.When($"{baseUrl}/types/{Models.Article.Codename}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\article-type.json")));
        }

        private void MockTypes()
        {
            mockHttp.When($"{baseUrl}/types?skip=2")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\types.json")));
        }

        private void MockElement()
        {
            mockHttp.When($"{baseUrl}/types/{Models.Article.Codename}/elements/{Models.Article.TitleCodename}")
                .Respond("application/json", "{'type':'text','name':'Title','codename':'title'}");
        }

        private void MockTaxonomy()
        {
            mockHttp.When($"{baseUrl}/taxonomies/personas")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\taxonomies_personas.json")));
        }

        private void MockTaxonomies()
        {
            mockHttp.When($"{baseUrl}/taxonomies")
                .WithQueryString("skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\taxonomies_multiple.json")));
        }

        private RequestCount MockItemAndLogHttpRequests(RequestCount actualHttpRequests)
        {
            mockHttp.When($"{baseUrl}/items/{"coffee_beverages_explained"}?language=es-ES")
                .Respond("application/json", (request) => GetMockedJsonAndLog(actualHttpRequests));

            return actualHttpRequests;
        }

        private FileStream GetMockedJsonAndLog(RequestCount actualHttpRequests)
        {
            actualHttpRequests.Value++;

            return GetMockedItemJsonStream();
        }

        private FileStream GetMockedItemJsonStream()
        {
            return File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\coffee_beverages_explained.json"));
        }

        private static void AssertItemSingleDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_TYPED_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_RUNTIME_TYPED_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_VARIANT_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "americano", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "how_to_make_a_cappuccino", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "personas", Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_IDENTIFIER }, dependencies);
            Assert.Equal(23, dependencies.Count());
        }

        private static void AssertItemListingDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.All(dependencies, (d) => new[] { "about_us", "aeropress", "how_we_source_our_coffees", "how_we_roast_our_coffees", "our_philosophy", "manufacturer", "product_status" }.Contains(d.Codename));
            Assert.Equal(39, dependencies.Count());
        }

        private static void AssertTaxonomySingleDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.Equal(2, dependencies.Count());
            Assert.All(dependencies, d => d.Codename.Equals("personas", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("taxonomy", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("taxonomy_json", StringComparison.Ordinal));
        }

        private static void AssertTaxonomyListingDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.All(dependencies, (d) => new[] { "personas", "processing", "product_status" }.Contains(d.Codename));
            Assert.Equal(6, dependencies.Count());
        }

        private static void AssertTypeSingleDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.Equal(22, dependencies.Count());
            Assert.Equal(2, dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Count());
            Assert.Contains(dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Select(d => d.Type), i => i.Equals("content_element", StringComparison.Ordinal));
            Assert.Contains(dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Select(d => d.Type), i => i.Equals("content_element_json", StringComparison.Ordinal));
            Assert.Equal(10, dependencies.Where(d => d.Type.Equals("content_element")).Count());
            Assert.Equal(10, dependencies.Where(d => d.Type.Equals("content_element_json")).Count());
        }

        private static void AssertTypeListingDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.Equal(328, dependencies.Count());
            Assert.Equal(13, dependencies.Where(d => d.Type.Equals("content_type", StringComparison.Ordinal)).Count());
            Assert.Equal(13, dependencies.Where(d => d.Type.Equals("content_type_json", StringComparison.Ordinal)).Count());
            Assert.Equal(151, dependencies.Where(d => d.Type.Equals("content_element", StringComparison.Ordinal)).Count());
            Assert.Equal(151, dependencies.Where(d => d.Type.Equals("content_element_json", StringComparison.Ordinal)).Count());
        }
    }

    public class RequestCount
    {
        public int Value = 0;
    }
}

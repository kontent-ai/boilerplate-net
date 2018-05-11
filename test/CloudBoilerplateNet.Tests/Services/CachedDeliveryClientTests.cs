using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
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
            var client = GetCachedDeliveryClient(MockItem);
            var response = await client.GetItemAsync("coffee_beverages_explained", new LanguageParameter("es-ES"));
            var dependencies = client.GetContentItemSingleDependencies(response);

            AssertItemSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentItemSingleJsonDependencies()
        {
            var client = GetCachedDeliveryClient(MockItem);
            var response = await client.GetItemJsonAsync("coffee_beverages_explained", "language=es-ES");
            var dependencies = client.GetContentItemSingleJsonDependencies(response);

            AssertItemSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentItemListingDependencies()
        {
            var client = GetCachedDeliveryClient(MockItems);
            var response = await client.GetItemsAsync(new LimitParameter(2), new SkipParameter(1));
            var dependencies = client.GetContentItemListingDependencies(response);
            AssertItemListingDependencies(dependencies);
        }


        [Fact]
        public async void GetsContentItemListingJsonDependencies()
        {
            var client = GetCachedDeliveryClient(MockItems);
            var response = await client.GetItemsJsonAsync("limit=2", "skip=1");
            var dependencies = client.GetContentItemListingJsonDependencies(response);
            AssertItemListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsContentElementDependency()
        {
            var client = GetCachedDeliveryClient(MockElement);
            var response = await client.GetContentElementAsync(Models.Article.Codename, Models.Article.TitleCodename);
            var dependencies = client.GetContentElementDependencies(response);

            Assert.Equal(2, dependencies.Count());
            Assert.All(dependencies, d => d.Codename.Equals("text|title", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("content_element", StringComparison.Ordinal));
            Assert.Contains(dependencies, d => d.Type.Equals("content_element_json", StringComparison.Ordinal));
        }

        [Fact]
        public async void GetsTaxonomySingleDependency()
        {
            var client = GetCachedDeliveryClient(MockTaxonomy);
            var response = await client.GetTaxonomyAsync("personas");
            var dependencies = client.GetTaxonomySingleDependency(response);

            AssertTaxonomySingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomySingleJsonDependency()
        {
            var client = GetCachedDeliveryClient(MockTaxonomy);
            var response = await client.GetTaxonomyJsonAsync("personas");
            var dependencies = client.GetTaxonomySingleJsonDependency(response);

            AssertTaxonomySingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomyListingDependencies()
        {
            var client = GetCachedDeliveryClient(MockTaxonomies);
            var response = await client.GetTaxonomiesAsync(new SkipParameter(1));
            var dependencies = client.GetTaxonomyListingDependencies(response);

            AssertTaxonomyListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTaxonomyListingJsonDependencies()
        {
            var client = GetCachedDeliveryClient(MockTaxonomies);
            var response = await client.GetTaxonomiesJsonAsync("skip=1");
            var dependencies = client.GetTaxonomyListingJsonDependencies(response);

            AssertTaxonomyListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeSingleDependencies()
        {
            var client = GetCachedDeliveryClient(MockType);
            var response = await client.GetTypeAsync(Models.Article.Codename);
            var dependencies = client.GetTypeSingleDependencies(response);

            AssertTypeSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeSingleJsonDependencies()
        {
            var client = GetCachedDeliveryClient(MockType);
            var response = await client.GetTypeJsonAsync(Models.Article.Codename);
            var dependencies = client.GetTypeSingleJsonDependencies(response);

            AssertTypeSingleDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeListingDependencies()
        {
            var client = GetCachedDeliveryClient(MockTypes);
            var response = await client.GetTypesAsync(new SkipParameter(2));
            var dependencies = client.GetTypeListingDependencies(response);

            AssertTypeListingDependencies(dependencies);
        }

        [Fact]
        public async void GetsTypeListingJsonDependencies()
        {
            var client = GetCachedDeliveryClient(MockTypes);
            var response = await client.GetTypesJsonAsync("skip=2");
            var dependencies = client.GetTypeListingJsonDependencies(response);

            AssertTypeListingDependencies(dependencies);
        }

        private CachedDeliveryClient GetCachedDeliveryClient(Action mockAction)
        {
            mockAction();
            var httpClient = mockHttp.ToHttpClient();

            var projectOptions = Options.Create(new ProjectOptions
            {
                CacheTimeoutSeconds = 60,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = guid
                }
            });

            var memoryCacheOptions = Options.Create(new MemoryCacheOptions
            {
                Clock = new TestClock(),
                ExpirationScanFrequency = new TimeSpan(0, 0, 5)
            });

            var cacheManager = new ReactiveCacheManager(projectOptions, new MemoryCache(memoryCacheOptions), new DependentFormatResolver(), new WebhookListener());

            return new CachedDeliveryClient(projectOptions, cacheManager, new DependentFormatResolver())
            {
                CodeFirstModelProvider = { TypeProvider = new Models.CustomTypeProvider() },
                HttpClient = httpClient
            };
        }

        private void MockItem()
        {
            mockHttp.When($"{baseUrl}/items/{"coffee_beverages_explained"}?language=es-ES")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\CachedDeliveryClient\\coffee_beverages_explained.json")));
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
            Assert.Equal(17, dependencies.Count());
        }

        private static void AssertItemListingDependencies(IEnumerable<IdentifierSet> dependencies)
        {
            Assert.All(dependencies, (d) => new[] { "about_us", "aeropress", "how_we_source_our_coffees", "how_we_roast_our_coffees", "our_philosophy", "manufacturer", "product_status" }.Contains(d.Codename));
            Assert.Equal(29, dependencies.Count());
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
            Assert.Equal(20, dependencies.Count());
            Assert.Equal(2, dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Count());
            Assert.Contains(dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Select(d => d.Type), i => i.Equals("content_element", StringComparison.Ordinal));
            Assert.Contains(dependencies.Where(d => d.Codename.Equals("taxonomy|personas", StringComparison.Ordinal)).Select(d => d.Type), i => i.Equals("content_element_json", StringComparison.Ordinal));
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
}

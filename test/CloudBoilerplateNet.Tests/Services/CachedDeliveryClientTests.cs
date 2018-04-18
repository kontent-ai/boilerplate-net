using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CloudBoilerplateNet.Helpers;
using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using KenticoCloud.Delivery;
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
            var response = await GetDeliveryClient(MockItem).GetItemAsync("coffee_beverages_explained", new LanguageParameter("es-ES"));
            var dependencies = CachedDeliveryClient.GetContentItemSingleDependencies(response);

            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "americano", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "how_to_make_a_cappuccino", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_IDENTIFIER }, dependencies);
            Assert.Equal(3, dependencies.Count());
        }

        [Fact]
        public async void GetsContentItemSingleJsonDependencies()
        {
            var response = await GetDeliveryClient(MockItem).GetItemJsonAsync("coffee_beverages_explained", "language=es-ES");
            var dependencies = CachedDeliveryClient.GetContentItemSingleJsonDependencies(response);

            Assert.Contains(new IdentifierSet { Codename = "coffee_beverages_explained", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "americano", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "how_to_make_a_cappuccino", Type = KenticoCloudCacheHelper.CONTENT_ITEM_SINGLE_JSON_IDENTIFIER }, dependencies);
            Assert.Contains(new IdentifierSet { Codename = "personas", Type = KenticoCloudCacheHelper.TAXONOMY_GROUP_SINGLE_JSON_IDENTIFIER }, dependencies);
            Assert.Equal(4, dependencies.Count());
        }

        [Fact]
        public async void GetsContentItemListingDependencies()
        {
            var response = await GetDeliveryClient(MockItems).GetItemsAsync(new LimitParameter(2), new SkipParameter(1));
            var dependencies = CachedDeliveryClient.GetContentItemListingDependencies(response);

            Assert.All(dependencies, (d) => new[] { "about_us", "aeropress", "how_we_source_our_coffees", "how_we_roast_our_coffees", "our_philosophy" }.Contains(d.Codename));
            Assert.Equal(5, dependencies.Count());
        }

        [Fact]
        public async void GetsContentItemListingJsonDependencies()
        {
            var response = await GetDeliveryClient(MockItems).GetItemsJsonAsync("limit=2", "skip=1");
            var dependencies = CachedDeliveryClient.GetContentItemListingJsonDependencies(response);

            Assert.All(dependencies, (d) => new[] { "about_us", "aeropress", "how_we_source_our_coffees", "how_we_roast_our_coffees", "our_philosophy" }.Contains(d.Codename));
            Assert.Equal(5, dependencies.Count());
        }

        [Fact]
        public async void GetsContentElementDependency()
        {
            var response = await GetDeliveryClient(MockElement).GetContentElementAsync(Models.Article.Codename, Models.Article.TitleCodename);
            var dependencies = CachedDeliveryClient.GetContentElementDependency(response);

            Assert.Single(dependencies);
            Assert.Equal("text|title", dependencies.First().Codename);
            Assert.Equal("content_element", dependencies.First().Type);
        }

        [Fact]
        public async void GetsTaxonomySingleDependency()
        {
            var response = await GetDeliveryClient(MockTaxonomy).GetTaxonomyAsync("personas");
            var dependencies = CachedDeliveryClient.GetTaxonomySingleDependency(response);

            Assert.Single(dependencies);
            Assert.Equal("personas", dependencies.First().Codename);
            Assert.Equal("taxonomy", dependencies.First().Type);
        }

        [Fact]
        public async void GetsTaxonomySingleJsonDependency()
        {
            var response = await GetDeliveryClient(MockTaxonomy).GetTaxonomyJsonAsync("personas");
            var dependencies = CachedDeliveryClient.GetTaxonomySingleJsonDependency(response);

            Assert.Single(dependencies);
            Assert.Equal("personas", dependencies.First().Codename);
            Assert.Equal("taxonomy_json", dependencies.First().Type);
        }

        [Fact]
        public async void GetsTaxonomyListingDependencies()
        {
            var response = await GetDeliveryClient(MockTaxonomies).GetTaxonomiesAsync(new SkipParameter(1));
            var dependencies = CachedDeliveryClient.GetTaxonomyListingDependencies(response);

            Assert.All(dependencies, (d) => new[] { "personas", "processing", "product_status" }.Contains(d.Codename));
            Assert.Equal(3, dependencies.Count());
        }

        [Fact]
        public async void GetsTaxonomyListingJsonDependencies()
        {
            var response = await GetDeliveryClient(MockTaxonomies).GetTaxonomiesJsonAsync("skip=1");
            var dependencies = CachedDeliveryClient.GetTaxonomyListingJsonDependencies(response);

            Assert.All(dependencies, (d) => new[] { "personas", "processing", "product_status" }.Contains(d.Codename));
            Assert.Equal(3, dependencies.Count());
        }

        [Fact]
        public async void GetsTypeSingleDependencies()
        {
            var response = await GetDeliveryClient(MockType).GetTypeAsync(Models.Article.Codename);
            var dependencies = CachedDeliveryClient.GetTypeSingleDependencies(response);

            Assert.Equal(11, dependencies.Count());
            Assert.Single(dependencies.Where(d => d.Type.Equals("content_type", StringComparison.Ordinal)));
            Assert.Equal(10, dependencies.Where(d => d.Type.Equals("content_element", StringComparison.Ordinal)).Count());
        }

        [Fact]
        public async void GetsTypeSingleJsonDependencies()
        {
            var response = await GetDeliveryClient(MockType).GetTypeJsonAsync(Models.Article.Codename);
            var dependencies = CachedDeliveryClient.GetTypeSingleJsonDependencies(response);

            Assert.Equal(11, dependencies.Count());
            Assert.Single(dependencies.Where(d => d.Type.Equals("content_type_json", StringComparison.Ordinal)));
            Assert.Equal(10, dependencies.Where(d => d.Type.Equals("content_element_json", StringComparison.Ordinal)).Count());
        }

        [Fact]
        public async void GetsTypeListingDependencies()
        {
            var response = await GetDeliveryClient(MockTypes).GetTypesAsync(new SkipParameter(2));
            var dependencies = CachedDeliveryClient.GetTypeListingDependencies(response);

            Assert.Equal(164, dependencies.Count());
            Assert.Equal(13, dependencies.Where(d => d.Type.Equals("content_type", StringComparison.Ordinal)).Count());
            Assert.Equal(151, dependencies.Where(d => d.Type.Equals("content_element", StringComparison.Ordinal)).Count());
        }

        [Fact]
        public async void GetsTypeListingJsonDependencies()
        {
            var response = await GetDeliveryClient(MockTypes).GetTypesJsonAsync("skip=2");
            var dependencies = CachedDeliveryClient.GetTypeListingJsonDependencies(response);

            Assert.Equal(164, dependencies.Count());
            Assert.Equal(13, dependencies.Where(d => d.Type.Equals("content_type_json", StringComparison.Ordinal)).Count());
            Assert.Equal(151, dependencies.Where(d => d.Type.Equals("content_element_json", StringComparison.Ordinal)).Count());
        }

        private IDeliveryClient GetDeliveryClient(Action mockAction)
        {
            mockAction();
            var httpClient = mockHttp.ToHttpClient();

            return new DeliveryClient(guid)
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
    }
}

using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

using static Kentico.Kontent.Boilerplate.Tests.Caching.ResponseHelper;

namespace Kentico.Kontent.Boilerplate.Tests.Caching.Default
{
    public class CachingDeliveryClientTests
    {
        #region GetItemJson

        [Fact]
        public async Task GetItemJsonAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            var secondResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetItem

        [Fact]
        public async Task GetItemAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            var secondResponse = await scenario.CachingClient.GetItemAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetItemTyped

        [Fact]
        public async Task GetItemTypedAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            var secondResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetItemsJson

        [Fact]
        public async Task GetItemsJsonAsync_ResponseIsCached()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            var secondResponse = await scenario.CachingClient.GetItemsJsonAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetItems

        [Fact]
        public async Task GetItemsAsync_ResponseIsCached()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            var secondResponse = await scenario.CachingClient.GetItemsAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetItemsTyped

        [Fact]
        public async Task GetItemsTypedAsync_ResponseIsCached()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsAsync<TestItem>();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            var secondResponse = await scenario.CachingClient.GetItemsAsync<TestItem>();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetTypeJson

        [Fact]
        public async Task GetTypeJsonAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"types/{codename}";
            var type = CreateType(codename, "Original");
            var updatedType = CreateType(codename, "Updated");

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, type).Build();
            var firstResponse = await scenario.CachingClient.GetTypeJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedType).Build();
            var secondResponse = await scenario.CachingClient.GetTypeJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetType

        [Fact]
        public async Task GetTypeAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"types/{codename}";
            var type = CreateType(codename, "Original");
            var updatedType = CreateType(codename, "Updated");

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, type).Build();
            var firstResponse = await scenario.CachingClient.GetTypeAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedType).Build();
            var secondResponse = await scenario.CachingClient.GetTypeAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetTypesJson

        [Fact]
        public async Task GetTypesJsonAsync_ResponseIsCached()
        {
            var url = "types";
            var typeA = CreateType("a");
            var types = CreateTypesResponse(new[] { typeA });
            var updatedTypes = CreateTypesResponse(new[] { typeA, CreateType("b") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, types).Build();
            var firstResponse = await scenario.CachingClient.GetTypesJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTypes).Build();
            var secondResponse = await scenario.CachingClient.GetTypesJsonAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion

        #region GetTypes

        [Fact]
        public async Task GetTypesAsync_ResponseIsCached()
        {
            var url = "types";
            var typeA = CreateType("a");
            var types = CreateTypesResponse(new[] { typeA });
            var updatedTypes = CreateTypesResponse(new[] { typeA, CreateType("b") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, types).Build();
            var firstResponse = await scenario.CachingClient.GetTypesAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTypes).Build();
            var secondResponse = await scenario.CachingClient.GetTypesAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetContentElement

        [Fact]
        public async Task GetContentElementAsync_ResponseIsCached()
        {
            const string typeCodename = "type";
            const string elementCodename = "element";
            var url = $"types/{typeCodename}/elements/{elementCodename}";
            var contentElement = CreateContentElement(elementCodename, "Original");
            var updatedContentElement = CreateContentElement(elementCodename, "Updated");

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, contentElement).Build();
            var firstResponse = await scenario.CachingClient.GetContentElementAsync(typeCodename, elementCodename);

            scenario = scenarioBuilder.WithResponse(url, updatedContentElement).Build();
            var secondResponse = await scenario.CachingClient.GetContentElementAsync(typeCodename, elementCodename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetTaxonomyJson

        [Fact]
        public async Task GetTaxonomyJsonAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"taxonomies/{codename}";
            var taxonomy = CreateTaxonomy(codename, new[] { "term1" });
            var updatedTaxonomy = CreateTaxonomy(codename, new[] { "term1", "term2" });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomy).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomyJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomy).Build();
            var secondResponse = await scenario.CachingClient.GetTaxonomyJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetTaxonomy

        [Fact]
        public async Task GetTaxonomyAsync_ResponseIsCached()
        {
            const string codename = "codename";
            var url = $"taxonomies/{codename}";
            var taxonomy = CreateTaxonomy(codename, new[] { "term1" });
            var updatedTaxonomy = CreateTaxonomy(codename, new[] { "term1", "term2" });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomy).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomyAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomy).Build();
            var secondResponse = await scenario.CachingClient.GetTaxonomyAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetTaxonomiesJson

        [Fact]
        public async Task GetTaxonomiesJsonAsync_ResponseIsCached()
        {
            var url = "taxonomies";
            var taxonomyA = CreateTaxonomy("a", new[] { "term1" });
            var taxonomies = CreateTaxonomiesResponse(new[] { taxonomyA });
            var updatedTaxonomies = CreateTaxonomiesResponse(new[] { taxonomyA, CreateTaxonomy("b", new[] { "term3" }) });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomies).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomiesJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomies).Build();
            var secondResponse = await scenario.CachingClient.GetTaxonomiesJsonAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }
        
        #endregion

        #region GetTaxonomies

        [Fact]
        public async Task GetTaxonomiesAsync_ResponseIsCached()
        {
            var url = "taxonomies";
            var taxonomyA = CreateTaxonomy("a", new[] { "term1" });
            var taxonomies = CreateTaxonomiesResponse(new[] { taxonomyA });
            var updatedTaxonomies = CreateTaxonomiesResponse(new[] { taxonomyA, CreateTaxonomy("b", new[] { "term3" }) });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomies).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomiesAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomies).Build();
            var secondResponse = await scenario.CachingClient.GetTaxonomiesAsync();

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        #endregion
    }
}
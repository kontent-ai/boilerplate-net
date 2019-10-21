using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Delivery;
using Xunit;

using static Kentico.Kontent.Boilerplate.Tests.Caching.ResponseHelper;

namespace Kentico.Kontent.Boilerplate.Tests.Caching.Webhooks
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

        [Fact]
        public async Task GetItemJsonAsync_InvalidatedByItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(codename));
            var secondResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemJsonAsync_InvalidatedByLinkedItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            const string modularCodename = "modular_codename";
            var modularContent = new[] { (modularCodename, CreateItem(modularCodename)) };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(modularCodename));
            var secondResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemJsonAsync_NotInvalidatedByComponentDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var component = CreateComponent();
            var modularContent = new[] { component };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(component.codename));
            var secondResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        [Fact]
        public async Task GetItemJsonAsync_TooManyItems_InvalidatedByItemsDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var modularContent = Enumerable.Range(1, 51).Select(i => $"modular_{i}").Select(cn => (cn, CreateItem(cn))).ToList();
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetItemAsync_InvalidatedByItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(codename));
            var secondResponse = await scenario.CachingClient.GetItemAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemAsync_InvalidatedByLinkedItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            const string modularCodename = "modular_codename";
            var modularContent = new[] { (modularCodename, CreateItem(modularCodename)) };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(modularCodename));
            var secondResponse = await scenario.CachingClient.GetItemAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemAsync_NotInvalidatedByComponentDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var component = CreateComponent();
            var modularContent = new[] { component };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(component.codename));
            var secondResponse = await scenario.CachingClient.GetItemAsync(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        [Fact]
        public async Task GetItemAsync_TooManyItems_InvalidatedByItemsDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var modularContent = Enumerable.Range(1, 51).Select(i => $"modular_{i}").Select(cn => (cn, CreateItem(cn))).ToList();
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetItemTypedAsync_InvalidatedByItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var item = CreateItemResponse(CreateItem(codename, "original"));
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"));

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(codename));
            var secondResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemTypedAsync_InvalidatedByLinkedItemDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            const string modularCodename = "modular_codename";
            var modularContent = new[] { (modularCodename, CreateItem(modularCodename)) };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(modularCodename));
            var secondResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemTypedAsync_NotInvalidatedByComponentDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var component = CreateComponent();
            var modularContent = new[] { component };
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemDependencyKey(component.codename));
            var secondResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            firstResponse.Should().NotBeNull();
            firstResponse.Should().BeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(1);
        }

        [Fact]
        public async Task GetItemTypedAsync_TooManyItems_InvalidatedByItemsDependency()
        {
            const string codename = "codename";
            var url = $"items/{codename}";
            var modularContent = Enumerable.Range(1, 51).Select(i => $"modular_{i}").Select(cn => (cn, CreateItem(cn))).ToList();
            var item = CreateItemResponse(CreateItem(codename, "original"), modularContent);
            var updatedItem = CreateItemResponse(CreateItem(codename, "updated"), modularContent);

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, item).Build();
            var firstResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedItem).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemAsync<TestItem>(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetItemsJsonAsync_InvalidatedByItemDependency()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemsJsonAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetItemsAsync_InvalidatedByItemDependency()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemsAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetItemsTypedAsync_InvalidatedByItemsDependency()
        {
            var url = "items";
            var itemB = CreateItem("b", "original");
            var items = CreateItemsResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedItems = CreateItemsResponse(new[] { CreateItem("a", "updated"), itemB });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, items).Build();
            var firstResponse = await scenario.CachingClient.GetItemsAsync<TestItem>();

            scenario = scenarioBuilder.WithResponse(url, updatedItems).Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            var secondResponse = await scenario.CachingClient.GetItemsAsync<TestItem>();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        #endregion

        #region GetItemsFeed

        [Fact]
        public async Task GetItemsFeed_ResponseIsCached()
        {
            var url = "items-feed";
            var headersWithContinuation = new[] { new KeyValuePair<string, string>("X-Continuation", "continuationToken") };
            var itemB = CreateItem("b", "original");
            var partOne = CreateItemsFeedResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedPartOne = CreateItemsFeedResponse(new[] { CreateItem("a", "updated"), itemB });
            var partTwo = CreateItemsFeedResponse(new[] { CreateItem("c", "original") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, partOne, headersWithContinuation)
                .Build();
            var feed = scenario.CachingClient.GetItemsFeed();
            var responsesA = new List<DeliveryItemsFeedResponse>();
            while (feed.HasMoreResults)
            {
                responsesA.Add(await feed.FetchNextBatchAsync());
            }

            scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, updatedPartOne, headersWithContinuation)
                .Build();

            feed = scenario.CachingClient.GetItemsFeed();
            var responsesB = new List<DeliveryItemsFeedResponse>();
            while (feed.HasMoreResults)
            {
                responsesB.Add(await feed.FetchNextBatchAsync());
            }

            responsesA.Should().NotBeNull();
            responsesA.Should().BeEquivalentTo(responsesB);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemsFeed_InvalidatedByItemsDependency()
        {
            var url = "items-feed";
            var headersWithContinuation = new[] { new KeyValuePair<string, string>("X-Continuation", "continuationToken") };
            var itemB = CreateItem("b", "original");
            var partOne = CreateItemsFeedResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedPartOne = CreateItemsFeedResponse(new[] { CreateItem("a", "updated"), itemB });
            var partTwo = CreateItemsFeedResponse(new[] { CreateItem("c", "original") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, partOne, headersWithContinuation)
                .Build();
            var feed = scenario.CachingClient.GetItemsFeed();
            var responsesA = new List<DeliveryItemsFeedResponse>();
            while (feed.HasMoreResults)
            {
                responsesA.Add(await feed.FetchNextBatchAsync());
            }

            scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, updatedPartOne, headersWithContinuation)
                .Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            feed = scenario.CachingClient.GetItemsFeed();
            var responsesB = new List<DeliveryItemsFeedResponse>();
            while (feed.HasMoreResults)
            {
                responsesB.Add(await feed.FetchNextBatchAsync());
            }

            responsesA.Should().NotBeNull();
            responsesB.Should().NotBeNull();
            responsesA.Should().NotBeEquivalentTo(responsesB);
            scenario.GetRequestCount(url).Should().Be(4);
        }

        #endregion

        #region GetItemsFeedTyped

        [Fact]
        public async Task GetItemsFeedTyped_ResponseIsCached()
        {
            var url = "items-feed";
            var headersWithContinuation = new[] { new KeyValuePair<string, string>("X-Continuation", "continuationToken") };
            var itemB = CreateItem("b", "original");
            var partOne = CreateItemsFeedResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedPartOne = CreateItemsFeedResponse(new[] { CreateItem("a", "updated"), itemB });
            var partTwo = CreateItemsFeedResponse(new[] { CreateItem("c", "original") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, partOne, headersWithContinuation)
                .Build();
            var feed = scenario.CachingClient.GetItemsFeed<TestItem>();
            var responsesA = new List<DeliveryItemsFeedResponse<TestItem>>();
            while (feed.HasMoreResults)
            {
                responsesA.Add(await feed.FetchNextBatchAsync());
            }

            scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, updatedPartOne, headersWithContinuation)
                .Build();

            feed = scenario.CachingClient.GetItemsFeed<TestItem>();
            var responsesB = new List<DeliveryItemsFeedResponse<TestItem>>();
            while (feed.HasMoreResults)
            {
                responsesB.Add(await feed.FetchNextBatchAsync());
            }

            responsesA.Should().NotBeNull();
            responsesA.Should().BeEquivalentTo(responsesB);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        [Fact]
        public async Task GetItemsFeedTyped_InvalidatedByItemsDependency()
        {
            var url = "items-feed";
            var headersWithContinuation = new[] { new KeyValuePair<string, string>("X-Continuation", "continuationToken") };
            var itemB = CreateItem("b", "original");
            var partOne = CreateItemsFeedResponse(new[] { CreateItem("a", "original"), itemB });
            var updatedPartOne = CreateItemsFeedResponse(new[] { CreateItem("a", "updated"), itemB });
            var partTwo = CreateItemsFeedResponse(new[] { CreateItem("c", "original") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, partOne, headersWithContinuation)
                .Build();
            var feed = scenario.CachingClient.GetItemsFeed<TestItem>();
            var responsesA = new List<DeliveryItemsFeedResponse<TestItem>>();
            while (feed.HasMoreResults)
            {
                responsesA.Add(await feed.FetchNextBatchAsync());
            }

            scenario = scenarioBuilder
                .WithResponse(url, headersWithContinuation, partTwo, null)
                .WithResponse(url, null, updatedPartOne, headersWithContinuation)
                .Build();
            scenario.InvalidateDependency(CacheHelper.GetItemsDependencyKey());
            feed = scenario.CachingClient.GetItemsFeed<TestItem>();
            var responsesB = new List<DeliveryItemsFeedResponse<TestItem>>();
            while (feed.HasMoreResults)
            {
                responsesB.Add(await feed.FetchNextBatchAsync());
            }

            responsesA.Should().NotBeNull();
            responsesB.Should().NotBeNull();
            responsesA.Should().NotBeEquivalentTo(responsesB);
            scenario.GetRequestCount(url).Should().Be(4);
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

        [Fact]
        public async Task GetTypeJsonAsync_InvalidatedByTypesDependency()
        {
            const string codename = "codename";
            var url = $"types/{codename}";
            var type = CreateType(codename, "Original");
            var updatedType = CreateType(codename, "Updated");

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, type).Build();
            var firstResponse = await scenario.CachingClient.GetTypeJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedType).Build();
            scenario.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTypeJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTypeAsync_InvalidatedByTypesDependency()
        {
            const string codename = "codename";
            var url = $"types/{codename}";
            var type = CreateType(codename, "Original");
            var updatedType = CreateType(codename, "Updated");

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, type).Build();
            var firstResponse = await scenario.CachingClient.GetTypeAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedType).Build();
            scenario.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTypeAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTypesJsonAsync_InvalidatedByTypesDependency()
        {
            var url = "types";
            var typeA = CreateType("a");
            var types = CreateTypesResponse(new[] { typeA });
            var updatedTypes = CreateTypesResponse(new[] { typeA, CreateType("b") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, types).Build();
            var firstResponse = await scenario.CachingClient.GetTypesJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTypes).Build();
            scenario.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTypesJsonAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTypesAsync_InvalidatedByTypesDependency()
        {
            var url = "types";
            var typeA = CreateType("a");
            var types = CreateTypesResponse(new[] { typeA });
            var updatedTypes = CreateTypesResponse(new[] { typeA, CreateType("b") });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, types).Build();
            var firstResponse = await scenario.CachingClient.GetTypesAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTypes).Build();
            scenario.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTypesAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetContentElementAsync_InvalidatedByTypesDependency()
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
            scenario.InvalidateDependency(CacheHelper.GetTypesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetContentElementAsync(typeCodename, elementCodename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTaxonomyJsonAsync_InvalidatedByTypeDependency()
        {
            const string codename = "codename";
            var url = $"taxonomies/{codename}";
            var taxonomy = CreateTaxonomy(codename, new[] { "term1" });
            var updatedTaxonomy = CreateTaxonomy(codename, new[] { "term1", "term2" });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomy).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomyJsonAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomy).Build();
            scenario.InvalidateDependency(CacheHelper.GetTaxonomyDependencyKey(codename));
            var secondResponse = await scenario.CachingClient.GetTaxonomyJsonAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTaxonomyAsync_InvalidatedByTaxonomyDependency()
        {
            const string codename = "codename";
            var url = $"taxonomies/{codename}";
            var taxonomy = CreateTaxonomy(codename, new[] { "term1" });
            var updatedTaxonomy = CreateTaxonomy(codename, new[] { "term1", "term2" });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomy).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomyAsync(codename);

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomy).Build();
            scenario.InvalidateDependency(CacheHelper.GetTaxonomyDependencyKey(codename));
            var secondResponse = await scenario.CachingClient.GetTaxonomyAsync(codename);

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTaxonomiesJsonAsync_InvalidatedByTaxonomiesDependency()
        {
            var url = "taxonomies";
            var taxonomyA = CreateTaxonomy("a", new[] { "term1" });
            var taxonomies = CreateTaxonomiesResponse(new[] { taxonomyA });
            var updatedTaxonomies = CreateTaxonomiesResponse(new[] { taxonomyA, CreateTaxonomy("b", new[] { "term3" }) });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomies).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomiesJsonAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomies).Build();
            scenario.InvalidateDependency(CacheHelper.GetTaxonomiesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTaxonomiesJsonAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotEqual(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
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

        [Fact]
        public async Task GetTaxonomiesAsync_InvalidatedByTaxonomiesDependency()
        {
            var url = "taxonomies";
            var taxonomyA = CreateTaxonomy("a", new[] { "term1" });
            var taxonomies = CreateTaxonomiesResponse(new[] { taxonomyA });
            var updatedTaxonomies = CreateTaxonomiesResponse(new[] { taxonomyA, CreateTaxonomy("b", new[] { "term3" }) });

            var scenarioBuilder = new ScenarioBuilder();

            var scenario = scenarioBuilder.WithResponse(url, taxonomies).Build();
            var firstResponse = await scenario.CachingClient.GetTaxonomiesAsync();

            scenario = scenarioBuilder.WithResponse(url, updatedTaxonomies).Build();
            scenario.InvalidateDependency(CacheHelper.GetTaxonomiesDependencyKey());
            var secondResponse = await scenario.CachingClient.GetTaxonomiesAsync();

            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();
            firstResponse.Should().NotBeEquivalentTo(secondResponse);
            scenario.GetRequestCount(url).Should().Be(2);
        }

        #endregion
    }
}
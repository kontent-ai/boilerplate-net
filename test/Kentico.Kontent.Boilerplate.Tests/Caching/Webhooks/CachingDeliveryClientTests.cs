using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Boilerplate.Caching.Webhooks;
using Kentico.Kontent.Delivery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

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

        private static object CreateItemResponse(object item, IEnumerable<(string codename, object item)> modularContent = null) => new
        {
            item,
            modular_content = modularContent?.ToDictionary(x => x.codename, x => x.item) ?? new Dictionary<string, object>()
        };

        private static object CreateItem(string codename, string value = null) => new
        {
            elements = new Dictionary<string, object>
            {
                {
                    "title",
                    new
                    {
                        type = "text",
                        name= "Title",
                        value= value ?? string.Empty
                    }
                }
            },
            system = new
            {
                id = Guid.NewGuid().ToString(),
                codename,
                type = "test_item"
            }
        };

        private static (string codename, object item) CreateComponent()
        {
            var id = Guid.NewGuid().ToString();
            id = $"{id.Substring(0, 14)}0{id.Substring(15)}";
            var codename = $"n{id}";
            return (
                codename,
                new
                {
                    elements = new Dictionary<string, object>(),
                    system = new
                    {
                        id,
                        codename
                    }
                });
        }

        public class ScenarioBuilder
        {
            private readonly string _projectId = Guid.NewGuid().ToString();
            private readonly string _baseUrl;

            private readonly MemoryCache _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            private readonly Dictionary<string, int> _requestCounter = new Dictionary<string, int>();

            private readonly Dictionary<string, Action<MockHttpMessageHandler>> _configurations = new Dictionary<string, Action<MockHttpMessageHandler>>();

            public ScenarioBuilder()
            {
                _baseUrl = $"https://deliver.kontent.ai/{_projectId}/";
            }
            
            public ScenarioBuilder WithResponse(string relativeUrl, object responseObject)
            {
                var url = $"{_baseUrl}{relativeUrl.TrimStart('/')}";
                _configurations[url] = mockHttp => mockHttp.When(url).Respond("application/json", _ => CreateStreamAndCount(relativeUrl, responseObject));

                return this;
            }

            private Stream CreateStreamAndCount(string url, object responseObject)
            {
                _requestCounter[url] = _requestCounter.GetValueOrDefault(url) + 1;
                var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(responseObject));
                return new MemoryStream(bytes);
            }

            public Scenario Build()
            {
                var mockHttp = new MockHttpMessageHandler();
                foreach (var configure in _configurations.Values)
                {
                    configure?.Invoke(mockHttp);
                }
                return new Scenario(_memoryCache, mockHttp.ToHttpClient(), new DeliveryOptions { ProjectId = _projectId }, _requestCounter);
            }
        }

        public class Scenario
        {
            private readonly Dictionary<string, int> _requestCounter;
            private readonly CacheManager _cacheManager;
            public CachingDeliveryClient CachingClient { get; }

            public Scenario(IMemoryCache memoryCache, HttpClient httpClient, DeliveryOptions deliveryOptions, Dictionary<string, int> requestCounter)
            {
                _requestCounter = requestCounter;
                _cacheManager = new CacheManager(memoryCache, Options.Create(new CacheOptions()));
                var baseClient = DeliveryClientBuilder.WithOptions(_ => deliveryOptions).WithHttpClient(httpClient).Build();
                CachingClient = new CachingDeliveryClient(_cacheManager, baseClient);
            }

            public void InvalidateDependency(string dependency) => _cacheManager.InvalidateDependency(dependency);

            public int GetRequestCount(string url) => _requestCounter.GetValueOrDefault(url);
        }
    }
}
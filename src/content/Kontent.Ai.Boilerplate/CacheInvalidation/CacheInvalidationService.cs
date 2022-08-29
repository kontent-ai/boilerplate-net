using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.Boilerplate.CacheInvalidation;

internal class CacheInvalidationService : IHostedService
{
    private readonly IDeliveryCacheManager _cacheManager;
    private readonly IOptions<DeliveryOptions> _options;
    private string? _continuationToken;
    private readonly HttpClient _client;
    private Timer? _timer;


    public CacheInvalidationService(IDeliveryCacheManager cacheManager, IOptions<DeliveryOptions> options)
    {
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _client = new HttpClient();
        _continuationToken = CheckChangeFeed().Result.Item2;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(TryCacheInvalidation, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    private async Task<Tuple<IEnumerable<ChangeFeedResponseItem>?, string?>> CheckChangeFeed()
    {
        var changeFeedResponse = await _client.SendAsync(
            new HttpRequestMessage(method: HttpMethod.Get,
                    requestUri:
                    $"{_options.Value.ProductionEndpoint}/{_options.Value.ProjectId}/change-feed")
                { Headers = { { HeaderNames.Continuation, _continuationToken } } });
        var changeFeedItems = changeFeedResponse.StatusCode == HttpStatusCode.OK
            ? await JsonSerializer.DeserializeAsync<IEnumerable<ChangeFeedResponseItem>>(
                await changeFeedResponse.Content.ReadAsStreamAsync())
            : null;

        return new Tuple<IEnumerable<ChangeFeedResponseItem>?, string?>(changeFeedItems,
            changeFeedResponse.Headers.GetValues(HeaderNames.Continuation).FirstOrDefault());
    }
    
    private async void TryCacheInvalidation(object? state)
    {
        IEnumerable<ChangeFeedResponseItem>? changeFeed;
        do
        {
            (changeFeed, var continuationToken) = await CheckChangeFeed();
            if (continuationToken != null && continuationToken != _continuationToken)
                _continuationToken = continuationToken;
            if (changeFeed != null)  await InvalidateCache(itemsChanged: changeFeed);
        } while (changeFeed != null);
    }

    private async Task InvalidateCache(IEnumerable<ChangeFeedResponseItem> itemsChanged)
    {
        var dependencies = new HashSet<string>();
        {
            foreach (var item in itemsChanged)
            {
                dependencies.Add(CacheHelpers.GetItemDependencyKey(item.Codename));
            }

            dependencies.Add(CacheHelpers.GetItemsDependencyKey());
        }

        foreach (var dependency in dependencies)
        {
            await _cacheManager.InvalidateDependencyAsync(dependency);
        }
    }
}
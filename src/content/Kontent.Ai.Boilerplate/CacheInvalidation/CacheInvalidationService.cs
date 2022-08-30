using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
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
        _continuationToken = CacheInvalidationServiceHelper
            .CheckChangeFeed(client: _client, options: options, continuationToken: _continuationToken).Result.Item2;
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


    private async void TryCacheInvalidation(object? state)
    {
        IEnumerable<ChangeFeedResponseItem>? changeFeed;
        do
        {
            (changeFeed, var continuationToken) = await CacheInvalidationServiceHelper.CheckChangeFeed(client: _client,
                options: _options, continuationToken: _continuationToken);
            if (continuationToken != null && continuationToken != _continuationToken)
                _continuationToken = continuationToken;
            if (changeFeed != null)
                await CacheInvalidationServiceHelper.InvalidateCache(itemsChanged: changeFeed,
                    cacheManager: _cacheManager);
        } while (changeFeed != null);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.Boilerplate.CacheInvalidation;

internal static class CacheInvalidationServiceHelper
{
    internal static async Task<Tuple<IEnumerable<ChangeFeedResponseItem>?, string?>> CheckChangeFeed(
        IOptions<DeliveryOptions> options, string? continuationToken, HttpClient client)
    {
        var changeFeedResponse = await client.SendAsync(
            new HttpRequestMessage(method: HttpMethod.Get,
                    requestUri:
                    $"{options.Value.ProductionEndpoint}/{options.Value.ProjectId}/change-feed")
                { Headers = { { HeaderNames.Continuation, continuationToken } } });
        var changeFeedItems = changeFeedResponse.StatusCode == HttpStatusCode.OK
            ? await JsonSerializer.DeserializeAsync<IEnumerable<ChangeFeedResponseItem>>(
                await changeFeedResponse.Content.ReadAsStreamAsync())
            : null;

        return new Tuple<IEnumerable<ChangeFeedResponseItem>?, string?>(changeFeedItems,
            changeFeedResponse.Headers.GetValues(HeaderNames.Continuation).FirstOrDefault());
    }

    internal static async Task InvalidateCache(IEnumerable<ChangeFeedResponseItem> itemsChanged,
        IDeliveryCacheManager cacheManager)
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
            await cacheManager.InvalidateDependencyAsync(dependency);
        }
    }
}
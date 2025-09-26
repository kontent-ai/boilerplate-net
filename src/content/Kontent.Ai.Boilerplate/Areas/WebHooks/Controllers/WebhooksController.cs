using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Caching;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kontent.Ai.Boilerplate.Areas.WebHooks.Controllers
{
    [Area("WebHooks")]
    public class WebhooksController : Controller
    {
        public WebhooksController(IDeliveryCacheManager cacheManager, IDeliveryClient deliveryClient)
        {
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _deliveryClient = deliveryClient ?? throw new ArgumentNullException(nameof(deliveryClient));
        }

        private readonly IDeliveryCacheManager _cacheManager;
        private readonly IDeliveryClient _deliveryClient;

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookNotification? webhook)
        {
            if (webhook is null)
            {
                return Ok();
            }

            var dependencies = new HashSet<string>(StringComparer.Ordinal);
            var usedInFetchTasks = new List<Task<IEnumerable<string>>>();

            foreach (var notification in webhook.Notifications ?? Enumerable.Empty<WebhookModel>())
            {
                ProcessNotification(notification, dependencies, usedInFetchTasks);
            }

            await CollectUsedInDependencies(usedInFetchTasks, dependencies);
            await InvalidateCacheDependencies(dependencies);

            return Ok();
        }

        private void ProcessNotification(WebhookModel notification, HashSet<string> dependencies, List<Task<IEnumerable<string>>> usedInFetchTasks)
        {
            switch (notification)
            {
                // Content item variant: invalidate the item and listing; on publish/unpublish also invalidate items that reference it
                case { Message.ObjectType: "content_item", Data.System.Codename: var codename, Message.Action: var action }:
                    ProcessContentItemNotification(codename, action, dependencies, usedInFetchTasks);
                    break;

                // Taxonomy change: for created, invalidate the specific taxonomy and listing, otherwise also invalidate items and type listings
                case { Message.ObjectType: "taxonomy", Data.System.Codename: var codename, Message.Action: var action }:
                    ProcessTaxonomyNotification(codename, action, dependencies);
                    break;

                // Content type change: invalidate types listing and items listing
                case { Message.ObjectType: "content_type" }:
                    ProcessContentTypeNotification(dependencies);
                    break;

                // Language change: invalidate languages listing
                case { Message.ObjectType: "language" }:
                    ProcessLanguageNotification(dependencies);
                    break;

                // Asset and unknown types: no Delivery SDK dependency helpers; skip
            }
        }

        private void ProcessContentItemNotification(string codename, string action, HashSet<string> dependencies, List<Task<IEnumerable<string>>> usedInFetchTasks)
        {
            dependencies.Add(CacheHelpers.GetItemDependencyKey(codename));
            dependencies.Add(CacheHelpers.GetItemsDependencyKey());

            if (IsPublishOrUnpublishAction(action))
            {
                usedInFetchTasks.Add(GetItemUsedInDependencyKeys(codename));
            }
        }

        private static void ProcessTaxonomyNotification(string codename, string action, HashSet<string> dependencies)
        {
            dependencies.Add(CacheHelpers.GetTaxonomyDependencyKey(codename));
            dependencies.Add(CacheHelpers.GetTaxonomiesDependencyKey());

            if (!IsCreatedAction(action))
            {
                dependencies.Add(CacheHelpers.GetItemsDependencyKey());
                dependencies.Add(CacheHelpers.GetTypesDependencyKey());
            }
        }

        private static void ProcessContentTypeNotification(HashSet<string> dependencies)
        {
            dependencies.Add(CacheHelpers.GetTypesDependencyKey());
            dependencies.Add(CacheHelpers.GetItemsDependencyKey());
        }

        private static void ProcessLanguageNotification(HashSet<string> dependencies)
        {
            dependencies.Add(CacheHelpers.GetLanguagesDependencyKey());
        }

        private static bool IsPublishOrUnpublishAction(string action) =>
            string.Equals(action, "published", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(action, "unpublished", StringComparison.OrdinalIgnoreCase);

        private static bool IsCreatedAction(string action) =>
            string.Equals(action, "created", StringComparison.OrdinalIgnoreCase);

        private static async Task CollectUsedInDependencies(List<Task<IEnumerable<string>>> usedInFetchTasks, HashSet<string> dependencies)
        {
            if (usedInFetchTasks.Count == 0) return;

            var usedInResults = await Task.WhenAll(usedInFetchTasks);
            foreach (var key in usedInResults.SelectMany(result => result))
            {
                dependencies.Add(key);
            }
        }

        private async Task InvalidateCacheDependencies(HashSet<string> dependencies)
        {
            if (dependencies.Count == 0) return;

            var invalidations = dependencies.Select(key => _cacheManager.InvalidateDependencyAsync(key));
            await Task.WhenAll(invalidations);
        }

        private async Task<IEnumerable<string>> GetItemUsedInDependencyKeys(string codename)
        {
            List<string> dependencyKeys = [];
            var feed =  _deliveryClient.GetItemUsedIn(codename);

            while (feed.HasMoreResults)
            {
                IDeliveryItemsFeedResponse<IUsedInItem> response = await feed.FetchNextBatchAsync();
                
                foreach (IUsedInItem item in response.Items)
                {
                    dependencyKeys.Add(CacheHelpers.GetItemDependencyKey(item.System.Codename));
                }
            }
            return dependencyKeys;
        }
    }
}

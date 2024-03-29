﻿using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;

namespace Kontent.Ai.Boilerplate.Resolvers
{
    /// <summary>
    /// Sample implementation to resolve links to other content items in Rich Text
    /// </summary>
    /// <seealso cref="IContentLinkUrlResolver" />
    public class CustomContentLinkUrlResolver : IContentLinkUrlResolver
    {
        /// <summary>
        /// Resolves the link URL.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A relative URL to the page where the content is displayed</returns>
        public Task<string> ResolveLinkUrlAsync(IContentLink link)
        {
            return Task.FromResult($"/{link.UrlSlug}");
        }

        /// <summary>
        /// Resolves the broken link URL.
        /// </summary>
        /// <returns>A relative URL to the site's 404 page</returns>
        public Task<string> ResolveBrokenLinkUrlAsync()
        {
            // Resolves URLs to unavailable content items
            return Task.FromResult("/404");
        }
    }
}
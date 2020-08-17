using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;

namespace Kentico.Kontent.Boilerplate.Resolvers
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
        public Task<string> ResolveLinkUrl(IContentLink link)
        {
            return Task.FromResult($"/{link.UrlSlug}");
        }

        /// <summary>
        /// Resolves the broken link URL.
        /// </summary>
        /// <returns>A relative URL to the site's 404 page</returns>
        Task<string> IContentLinkUrlResolver.ResolveBrokenLinkUrl()
        {
            // Resolves URLs to unavailable content items
            return Task.FromResult("/404");
        }
    }
}
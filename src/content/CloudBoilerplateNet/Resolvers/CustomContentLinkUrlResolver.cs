using KenticoCloud.Delivery;

namespace CloudBoilerplateNet.Resolvers
{
    /// <summary>
    /// Sample implementation to resolve links to other content items in Rich Text
    /// </summary>
    /// <seealso cref="KenticoCloud.Delivery.IContentLinkUrlResolver" />
    public class CustomContentLinkUrlResolver : IContentLinkUrlResolver
    {
        /// <summary>
        /// Resolves the link URL.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>A relative URL to the page where the content is displayed</returns>
        public string ResolveLinkUrl(ContentLink link)
        {
            return $"/{link.UrlSlug}";
        }

        /// <summary>
        /// Resolves the broken link URL.
        /// </summary>
        /// <returns>A relative URL to the site's 404 page</returns>
        public string ResolveBrokenLinkUrl()
        {
            // Resolves URLs to unavailable content items
            return "/404";
        }
    }
}
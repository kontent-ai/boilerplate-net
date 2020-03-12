using System;
using System.Linq;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.ImageTransformation;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kentico.Kontent.Boilerplate.Helpers.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Application settings.
        /// </summary>
        public static ProjectOptions ProjectOptions { get; set; }

        /// <summary>
        /// Generates an IMG tag for an image file.
        /// </summary>
        /// <param name="htmlHelper">HTML helper.</param>
        /// <param name="asset">Asset</param>
        /// <param name="title">Title</param>
        /// <param name="cssClass">CSS class</param>
        /// <param name="width">Optional width size</param>
        /// <param name="height">Optional height size</param>
        /// <param name="sizes">Optional sizes img attribute</param>
        public static Microsoft.AspNetCore.Html.IHtmlContent AssetImage(this IHtmlHelper htmlHelper, Asset asset, string title = null, string cssClass = "", int? width = null, int? height = null, ResponsiveImageSizes sizes = null)
        {
            if (htmlHelper is null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (asset == null)
            {
                return HtmlString.Empty;
            }

            var imageUrlBuilder = new ImageUrlBuilder(asset.Url);
            var image = new TagBuilder("img");

            if (width.HasValue)
            {
                image.MergeAttribute("width", width.ToString());
                imageUrlBuilder = imageUrlBuilder.WithWidth(Convert.ToDouble(width));
            }

            if (height.HasValue)
            {
                image.MergeAttribute("height", height.ToString());
                imageUrlBuilder = imageUrlBuilder.WithHeight(Convert.ToDouble(height));
            }

            if (ProjectOptions.ResponsiveImagesEnabled && !width.HasValue && !height.HasValue)
            {
                image.MergeAttribute("srcset", GenerateSrcsetValue(asset.Url));

                if (sizes != null)
                {
                    image.MergeAttribute("sizes", sizes.GenerateSizesValue());
                }
            }

            image.MergeAttribute("src", $"{imageUrlBuilder.Url}");
            image.AddCssClass(cssClass);
            var titleToUse = title ?? asset.Description ?? string.Empty;
            image.MergeAttribute("alt", titleToUse);
            image.MergeAttribute("title", titleToUse);
            image.TagRenderMode = TagRenderMode.SelfClosing;

            return image;
        }

        private static string GenerateSrcsetValue(string imageUrl)
        {
            var imageUrlBuilder = new ImageUrlBuilder(imageUrl);

            return string.Join(",", ProjectOptions.ResponsiveWidths.Select(w
                => $"{imageUrlBuilder.WithWidth(Convert.ToDouble(w)).Url} {w}w"));
        }
    }
}
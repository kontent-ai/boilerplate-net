using System;
using System.Linq;
using KenticoCloud.Delivery;
using KenticoCloud.Delivery.ImageTransformation;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace CloudBoilerplateNet.Helpers.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Application settings.
        /// </summary>
        public static IOptions<ProjectOptions> ProjectOptions { get; set; }

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

            if (ProjectOptions.Value.ResponsiveImagesEnabled && !width.HasValue && !height.HasValue)
            {
                image.MergeAttribute("srcset", GenerateSrcsetValue(asset.Url));

                if (sizes != null)
                {
                    image.MergeAttribute("sizes", sizes.GenerateSizesValue());
                }
            }

            image.MergeAttribute("src", $"{imageUrlBuilder.Url}");
            image.AddCssClass(cssClass);
            string titleToUse = title ?? asset.Description ?? string.Empty;
            image.MergeAttribute("alt", titleToUse);
            image.MergeAttribute("title", titleToUse);

            return image;
        }

        private static string GenerateSrcsetValue(string imageUrl)
        {
            var imageUrlBuilder = new ImageUrlBuilder(imageUrl);

            return string.Join(",", ProjectOptions.Value.ResponsiveWidths.Select(w
                => $"{imageUrlBuilder.WithWidth(Convert.ToDouble(w)).Url} {w}w"));
        }
    }
}
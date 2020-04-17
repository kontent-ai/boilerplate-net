using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.ImageTransformation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.Boilerplate.TagHelpers
{
    [RestrictChildren("media-condition")]
    [HtmlTargetElement("img-asset", Attributes = "asset")]
    public class AssetTagHelper : TagHelper
    {
        /// <summary>
        /// Application settings.
        /// </summary>
        public IOptions<ProjectOptions> ProjectOptions { get; set; }

        [HtmlAttributeName("asset")]
        public Asset Asset { get; set; }

        [HtmlAttributeName("title")]
        public string Title { get; set; }

        [HtmlAttributeName("default-width")]
        public int DefaultWidth { get; set; }

        public AssetTagHelper(IOptions<ProjectOptions> projectOptions)
        {
            ProjectOptions = projectOptions;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Asset == null)
            {
                base.Process(context, output);
                return;
            }

            output.TagName = "img";
            output.TagMode = TagMode.SelfClosing;


            var image = new TagBuilder("img");

            if (ProjectOptions.Value.ResponsiveWidths != null && ProjectOptions.Value.ResponsiveWidths.Any() && context.AllAttributes["width"]?.Value == null && context.AllAttributes["height"]?.Value == null)
            {
                image.MergeAttribute("srcset", GenerateSrcsetValue(Asset.Url, ProjectOptions.Value.ResponsiveWidths));

                var sizes = new List<string>();
                context.Items.Add("sizes", sizes);
                await output.GetChildContentAsync();

                if (sizes != null)
                {
                    var s = string.Join(", ", sizes.Concat(new[] { $"{DefaultWidth}px" }));
                    image.MergeAttribute("sizes", s);
                }
            }

            image.MergeAttribute("src", $"{new ImageUrlBuilder(Asset.Url).Url}");
            var titleToUse = Title ?? Asset.Description ?? string.Empty;
            image.MergeAttribute("alt", titleToUse);
            image.MergeAttribute("title", titleToUse);
            output.MergeAttributes(image);
        }

        private static string GenerateSrcsetValue(string imageUrl, int[] widths)
        {
            return string.Join(",", widths.Select(w => $"{new ImageUrlBuilder(imageUrl).WithWidth(Convert.ToDouble(w)).Url} {w}w"));
        }
    }
}
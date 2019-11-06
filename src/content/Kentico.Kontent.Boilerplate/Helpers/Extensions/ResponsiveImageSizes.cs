using System.Collections.Generic;
using System.Linq;

namespace Kentico.Kontent.Boilerplate.Helpers.Extensions
{
    /// <summary>
    /// Represents the sizes attribute of an image.
    /// </summary>
    public class ResponsiveImageSizes
    {
        private readonly int _defaultWidth;
        private readonly List<MediaCondition> _mediaConditions;


        /// <summary>
        /// Creates new <see cref="ResponsiveImageSizes"/> instance with default width.
        /// </summary>
        /// <param name="defaultWidth">Default width in pixels.</param>
        public ResponsiveImageSizes(int defaultWidth)
        {
            _defaultWidth = defaultWidth;
            _mediaConditions = new List<MediaCondition>();
        }

        /// <summary>
        /// Adds the <paramref name="mediaCondition"/> into sizes attribute value.
        /// </summary>
        /// <param name="mediaCondition">Represents one media condition from image sizes attribute.</param>
        public ResponsiveImageSizes WithMediaCondition(MediaCondition mediaCondition)
        {
            _mediaConditions.Add(mediaCondition);

            return this;
        }

        /// <summary>
        /// Generates sizes attribute value.
        /// </summary>
        public string GenerateSizesValue() =>
            string.Join(", ", _mediaConditions.Select(mc => mc.ToString()).Concat(new [] { $"{_defaultWidth}px" }));
    }
}
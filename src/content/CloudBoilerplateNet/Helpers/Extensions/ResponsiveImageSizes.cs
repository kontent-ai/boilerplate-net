using System.Collections.Generic;
using System.Linq;

namespace CloudBoilerplateNet.Helpers.Extensions
{
    /// <summary>
    /// Represents the sizes attribute of an image.
    /// </summary>
    public class ResponsiveImageSizes
    {
        private readonly int _defaultWidth;
        private readonly List<MediaCondition> _mediaContitions;


        /// <summary>
        /// Creates new <see cref="ResponsiveImageSizes"/> instance with default width.
        /// </summary>
        /// <param name="defaultWidth">Default width in pixels.</param>
        public ResponsiveImageSizes(int defaultWidth)
        {
            _defaultWidth = defaultWidth;
            _mediaContitions = new List<MediaCondition>();
        }

        /// <summary>
        /// Adds the <paramref name="mediaCondition"/> into sizes attribute value.
        /// </summary>
        /// <param name="mediaCondition">Represents one media condition from image sizes attribute.</param>
        public ResponsiveImageSizes WithMediaCondition(MediaCondition mediaCondition)
        {
            _mediaContitions.Add(mediaCondition);

            return this;
        }

        /// <summary>
        /// Generates sizes attribute value.
        /// </summary>
        public string GenerateSizesValue() =>
            string.Join(", ", _mediaContitions.Select(mc => mc.ToString()).Concat(new string[] { $"{_defaultWidth}px" }));
    }
}
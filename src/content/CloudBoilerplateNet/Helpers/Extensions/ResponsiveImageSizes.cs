using System.Collections.Generic;
using System.Linq;

namespace CloudBoilerplateNet.Helpers.Extensions
{
    public class ResponsiveImageSizes
    {
        private readonly int _defaultWidth;
        private readonly List<MediaCondition> _mediaContitions;


        /// <summary>
        /// Creates new ResponsiveImageSizes instance with default width.
        /// </summary>
        /// <param name="defaultWidth">Default width in pixels.</param>
        public ResponsiveImageSizes(int defaultWidth)
        {
            _defaultWidth = defaultWidth;
            _mediaContitions = new List<MediaCondition>();
        }

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
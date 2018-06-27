namespace CloudBoilerplateNet.Helpers.Extensions
{
    /// <summary>
    /// Represents one media condition from image sizes attribute.
    /// </summary>
    public class MediaCondition
    {
        /// <summary>
        /// Minimum width of the window that should trigger usage of the <see cref="ImageWidth"/>.
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// Optional maximum width of the window that should trigger usage of the <see cref="ImageWidth"/>.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// The width of an image when the width of the window is between <see cref="MinWidth"/> and <see cref="MaxWidth"/>.
        /// </summary>
        public int ImageWidth { get; set; }

        public override string ToString()
        {
            var maxWidth = MaxWidth.HasValue ? $"(max-width: {MaxWidth.Value}px) and " : string.Empty; 

            return $"{maxWidth}(min-width: {MinWidth}px) {ImageWidth}px";
        }
    }
}
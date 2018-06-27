namespace CloudBoilerplateNet.Helpers.Extensions
{
    public class MediaCondition
    {
        public int MinWidth { get; set; }
        public int? MaxWidth { get; set; }
        public int ImageWidth { get; set; }

        public override string ToString()
        {
            var maxWidth = MaxWidth.HasValue ? $"(max-width: {MaxWidth.Value}px) and " : string.Empty; 

            return $"{maxWidth}(min-width: {MinWidth}px) {ImageWidth}px";
        }
    }
}
using System;
using System.Collections.Generic;
using KenticoCloud.Delivery;

namespace CloudBoilerplateNet.Models
{
    public partial class Article
    {
        public IEnumerable<TaxonomyTerm> Personas { get; set; }
        public string Title { get; set; }
        public IEnumerable<Asset> TeaserImage { get; set; }
        public DateTime? PostDate { get; set; }
        public string Summary { get; set; }
        public string BodyCopy { get; set; }
        public IEnumerable<ContentItem> RelatedArticles { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public ContentItemSystemAttributes System { get; set; }
    }
}
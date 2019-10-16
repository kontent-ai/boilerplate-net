using System.Linq;
using FluentAssertions;
using Kentico.Kontent.Boilerplate.Caching;
using Kentico.Kontent.Delivery;
using Xunit;

namespace Kentico.Kontent.Boilerplate.Tests.Caching
{
    public class CacheHelperTests
    {

        #region API keys

        [Fact]
        public void GetItemKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetItemJsonKey("codename"),
                CacheHelper.GetItemJsonKey("other_codename"),
                CacheHelper.GetItemJsonKey("codename", "depth=1"),
                CacheHelper.GetItemJsonKey("codename", "depth=2"),
                CacheHelper.GetItemJsonKey("codename", "system.type=article"),
                CacheHelper.GetItemKey("codename", Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemKey("other_codename", Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemKey("codename", new []{new DepthParameter(1)}),
                CacheHelper.GetItemKey("codename", new [] {new DepthParameter(2) }),
                CacheHelper.GetItemKey("codename", new []{new SystemTypeEqualsFilter("article") }),
                CacheHelper.GetItemTypedKey("codename", Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemTypedKey("other_codename", Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemTypedKey("codename", new []{new DepthParameter(1)}),
                CacheHelper.GetItemTypedKey("codename", new [] {new DepthParameter(2) }),
                CacheHelper.GetItemTypedKey("codename", new []{new SystemTypeEqualsFilter("article") })
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetItemsKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetItemsJsonKey(),
                CacheHelper.GetItemsJsonKey("depth=1"),
                CacheHelper.GetItemsJsonKey("depth=2"),
                CacheHelper.GetItemsJsonKey("system.type=article"),
                CacheHelper.GetItemsKey(Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemsKey(new []{new DepthParameter(1)}),
                CacheHelper.GetItemsKey(new [] {new DepthParameter(2) }),
                CacheHelper.GetItemsKey(new []{new SystemTypeEqualsFilter("article") }),
                CacheHelper.GetItemsTypedKey(Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetItemsTypedKey(new []{new DepthParameter(1)}),
                CacheHelper.GetItemsTypedKey(new [] {new DepthParameter(2) }),
                CacheHelper.GetItemsTypedKey(new []{new SystemTypeEqualsFilter("article") })
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetTypeKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetTypeJsonKey("codename"),
                CacheHelper.GetTypeJsonKey("other_codename"),
                CacheHelper.GetTypeKey("codename"),
                CacheHelper.GetTypeKey("other_codename")
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetTypesKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetTypesJsonKey(),
                CacheHelper.GetTypesJsonKey("skip=1"),
                CacheHelper.GetTypesJsonKey("skip=2"),
                CacheHelper.GetTypesJsonKey("limit=2"),
                CacheHelper.GetTypesKey(Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetTypesKey(new []{new  SkipParameter(1)}),
                CacheHelper.GetTypesKey(new [] {new SkipParameter(2)}),
                CacheHelper.GetTypesKey(new []{new LimitParameter(2)})
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetTaxonomyKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetTaxonomyJsonKey("codename"),
                CacheHelper.GetTaxonomyJsonKey("other_codename"),
                CacheHelper.GetTaxonomyKey("codename"),
                CacheHelper.GetTaxonomyKey("other_codename")
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetTaxonomiesKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetTaxonomiesJsonKey(),
                CacheHelper.GetTaxonomiesJsonKey("skip=1"),
                CacheHelper.GetTaxonomiesJsonKey("skip=2"),
                CacheHelper.GetTaxonomiesJsonKey("limit=2"),
                CacheHelper.GetTaxonomiesKey(Enumerable.Empty<IQueryParameter>()),
                CacheHelper.GetTaxonomiesKey(new []{new  SkipParameter(1)}),
                CacheHelper.GetTaxonomiesKey(new [] {new SkipParameter(2)}),
                CacheHelper.GetTaxonomiesKey(new []{new LimitParameter(2)})
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        [Fact]
        public void GetContentElementKey_WithDifferentValues_AreUnique()
        {
            var keys = new[]
            {
                CacheHelper.GetContentElementKey("type_codename", "element_codename"),
                CacheHelper.GetContentElementKey("type_codename", "other_element_codename"),
                CacheHelper.GetContentElementKey("other_type_codename", "element_codename")
            };

            keys.Distinct().Count().Should().Be(keys.Length);
        }

        #endregion
    }
}
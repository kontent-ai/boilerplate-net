using CloudBoilerplateNet.Controllers;
using KenticoCloud.Delivery;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CloudBoilerplateNet.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void IndexTests()
        {
            var options = Options.Create(new ProjectOptions()
            {
                KenticoCloudProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3"
            });

            var controller = new HomeController(options);
            var result = Task.Run(() => controller.Index()).Result;
            
            Assert.AreEqual(typeof(ReadOnlyCollection<ContentItem>), result.Model.GetType());
        }
    }
}

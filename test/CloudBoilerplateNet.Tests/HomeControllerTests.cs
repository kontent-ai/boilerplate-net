using CloudBoilerplateNet.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CloudBoilerplateNet.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void IndexTests()
        {
            var controller = new HomeController();
            var result = controller.Index();

            Assert.AreEqual(typeof(ViewResult), result.GetType());
        }
    }
}

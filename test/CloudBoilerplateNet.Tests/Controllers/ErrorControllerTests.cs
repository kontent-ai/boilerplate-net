using CloudBoilerplateNet.Controllers;
using NUnit.Framework;

namespace CloudBoilerplateNet.Tests
{
    [TestFixture]
    public class ErrorControllerTests
    {
        [TestCase(0, "~/Views/Error/GeneralError.cshtml")]
        [TestCase(404, "~/Views/Error/NotFound.cshtml")]
        [TestCase(500, "~/Views/Error/GeneralError.cshtml")]
        public void StatusTests(int statusCode, string errorPagePath)
        {
            var controller = new ErrorController();

            var result = controller.Status(statusCode);

            Assert.AreEqual(errorPagePath, result.ViewName);
        }
    }
}

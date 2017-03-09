using CloudBoilerplateNet.Controllers;
using Xunit;

namespace CloudBoilerplateNet.Tests
{
    public class ErrorControllerTests
    {
        [Theory]
        [InlineData(0, "~/Views/Error/GeneralError.cshtml")]
        [InlineData(404, "~/Views/Error/NotFound.cshtml")]
        [InlineData(500, "~/Views/Error/GeneralError.cshtml")]
        public void StatusTests(int statusCode, string errorPagePath)
        {
            var controller = new ErrorController();

            var result = controller.Status(statusCode);

            Assert.Equal(errorPagePath, result.ViewName);
        }
    }
}

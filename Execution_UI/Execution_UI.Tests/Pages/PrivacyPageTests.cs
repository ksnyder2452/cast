using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;

namespace Execution_UI.Tests.Pages
{
    public class PrivacyPageTests
    {
        [Fact]
        public void PrivacyModel_OnGet_DoesNotThrowException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PrivacyModel>>();
            var model = new PrivacyModel(mockLogger.Object);

            // Act & Assert - Should not throw
            var result = Record.Exception(() => model.OnGet());
            Assert.Null(result);
        }

        [Fact]
        public void PrivacyModel_OnGet_ExecutesSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PrivacyModel>>();
            var model = new PrivacyModel(mockLogger.Object);

            // Act
            model.OnGet();

            // Assert - Verify no exception and model is valid
            Assert.NotNull(model);
        }

        [Fact]
        public void PrivacyModel_Constructor_InitializesWithValidLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PrivacyModel>>();

            // Act
            var model = new PrivacyModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void PrivacyModel_CanCallOnGetMultipleTimes()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PrivacyModel>>();
            var model = new PrivacyModel(mockLogger.Object);

            // Act & Assert
            for (int i = 0; i < 3; i++)
            {
                var result = Record.Exception(() => model.OnGet());
                Assert.Null(result);
            }
        }

        [Fact]
        public void PrivacyModel_InheritsFromPageModel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PrivacyModel>>();

            // Act
            var model = new PrivacyModel(mockLogger.Object);

            // Assert
            Assert.IsAssignableFrom<PageModel>(model);
        }
    }
}

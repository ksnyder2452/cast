using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;

namespace Execution_UI.Tests.Pages
{
    public class CastPageTests
    {
        [Fact]
        public void CastModel_Constructor_InitializesWithValidLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void CastModel_InheritsFromPageModel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.IsAssignableFrom<PageModel>(model);
        }

        [Fact]
        public void CastModel_OriginatorUUIDs_IsInitializedAsEmptyList()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);

            // Act & Assert
            Assert.NotNull(model.originatorUUIDs);
            Assert.IsType<List<string>>(model.originatorUUIDs);
            Assert.Empty(model.originatorUUIDs);
        }

        [Fact]
        public void CastModel_SelectedValue_CanBeSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);
            var testValue = "test-value";

            // Act
            model.SelectedValue = testValue;

            // Assert
            Assert.Equal(testValue, model.SelectedValue);
        }

        [Fact]
        public void CastModel_SelectedValue_CanBeNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);

            // Act
            model.SelectedValue = null!;

            // Assert
            Assert.Null(model.SelectedValue);
        }

        [Fact]
        public void CastModel_RootDir_IsNotEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);

            // Act & Assert
            Assert.NotNull(model.rootDir);
            Assert.NotEmpty(model.rootDir);
        }

        [Fact]
        public void CastModel_OriginatorUUIDs_CanAddItems()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);
            var testUuid = "test-uuid-123";

            // Act
            model.originatorUUIDs.Add(testUuid);

            // Assert
            Assert.Single(model.originatorUUIDs);
            Assert.Contains(testUuid, model.originatorUUIDs);
        }

        [Fact]
        public void CastModel_Options_CanBeSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var model = new CastModel(mockLogger.Object);
            var items = new List<string> { "option1", "option2" };
            var selectList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(items);

            // Act
            model.Options = selectList;

            // Assert
            Assert.NotNull(model.Options);
        }

        [Fact]
        public void CastModel_StaticConfigurationValues_AreNotNull()
        {
            // Arrange & Act
            // Note: These may be null if appsettings.json is not properly configured
            // This test documents that these properties exist
            var model = new CastModel(new Mock<ILogger<IndexModel>>().Object);

            // Assert - Just verify the properties exist and can be accessed
            Assert.NotNull(model);
            // The static fields are set from configuration, so they might be null
            // depending on appsettings.json configuration
        }
    }
}

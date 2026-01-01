using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;

namespace Execution_UI.Tests.Pages
{
    /// <summary>
    /// Unit tests for CastModel page model.
    /// Tests initialization, properties, and data structure integrity.
    /// </summary>
    public class CastPageTests
    {
        private Mock<ILogger<IndexModel>> CreateMockLogger()
        {
            return new Mock<ILogger<IndexModel>>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidLogger_InitializesSuccessfully()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesOriginatorUUIDsList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.originatorUUIDs);
            Assert.IsType<List<string>>(model.originatorUUIDs);
            Assert.Empty(model.originatorUUIDs);
        }

        [Fact]
        public void Constructor_InitializesDisplayNamesList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.displayNames);
            Assert.IsType<List<string>>(model.displayNames);
            Assert.Empty(model.displayNames);
        }

        #endregion

        #region Inheritance Tests

        [Fact]
        public void CastModel_InheritsFromPageModel()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.IsAssignableFrom<PageModel>(model);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SelectedValue_CanBeSet()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var testValue = "test-value";

            // Act
            model.SelectedValue = testValue;

            // Assert
            Assert.Equal(testValue, model.SelectedValue);
        }

        [Fact]
        public void SelectedValue_CanBeSetToNull()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);

            // Act
            model.SelectedValue = null;

            // Assert
            Assert.Null(model.SelectedValue);
        }

        [Fact]
        public void SelectedValue_CanBeSetMultipleTimes()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var value1 = "value1";
            var value2 = "value2";

            // Act
            model.SelectedValue = value1;
            Assert.Equal(value1, model.SelectedValue);
            model.SelectedValue = value2;

            // Assert
            Assert.Equal(value2, model.SelectedValue);
        }

        [Fact]
        public void Options_CanBeSet()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var items = new List<string> { "option1", "option2", "option3" };
            var selectList = new SelectList(items);

            // Act
            model.Options = selectList;

            // Assert
            Assert.NotNull(model.Options);
            Assert.Equal(3, model.Options.Count());
        }

        [Fact]
        public void Options_CanBeNull()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);

            // Act
            model.Options = null;

            // Assert
            Assert.Null(model.Options);
        }

        #endregion

        #region Root Directory Tests

        [Fact]
        public void RootDir_IsNotEmpty()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.rootDir);
            Assert.NotEmpty(model.rootDir);
        }

        [Fact]
        public void RootDir_ContainsValidPath()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);

            // Act & Assert
            Assert.True(model.rootDir.Contains("..") || model.rootDir.Contains(Path.DirectorySeparatorChar.ToString()));
        }

        #endregion

        #region List Operations Tests

        [Fact]
        public void OriginatorUUIDs_CanAddItem()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var testUuid = "test-uuid-123";

            // Act
            model.originatorUUIDs.Add(testUuid);

            // Assert
            Assert.Single(model.originatorUUIDs);
            Assert.Contains(testUuid, model.originatorUUIDs);
        }

        [Fact]
        public void OriginatorUUIDs_CanAddMultipleItems()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var uuid1 = "uuid-1";
            var uuid2 = "uuid-2";
            var uuid3 = "uuid-3";

            // Act
            model.originatorUUIDs.Add(uuid1);
            model.originatorUUIDs.Add(uuid2);
            model.originatorUUIDs.Add(uuid3);

            // Assert
            Assert.Equal(3, model.originatorUUIDs.Count);
            Assert.Contains(uuid1, model.originatorUUIDs);
            Assert.Contains(uuid2, model.originatorUUIDs);
            Assert.Contains(uuid3, model.originatorUUIDs);
        }

        [Fact]
        public void DisplayNames_CanAddItem()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new CastModel(mockLogger.Object);
            var displayName = "Test Client";

            // Act
            model.displayNames.Add(displayName);

            // Assert
            Assert.Single(model.displayNames);
            Assert.Contains(displayName, model.displayNames);
        }

        [Fact]
        public void StartRun_InitializesAsEmptyList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.startRun);
            Assert.IsType<List<string>>(model.startRun);
            Assert.Empty(model.startRun);
        }

        [Fact]
        public void StopRun_InitializesAsEmptyList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.stopRun);
            Assert.IsType<List<string>>(model.stopRun);
            Assert.Empty(model.stopRun);
        }

        [Fact]
        public void PauseRun_InitializesAsEmptyList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.pauseRun);
            Assert.IsType<List<string>>(model.pauseRun);
            Assert.Empty(model.pauseRun);
        }

        [Fact]
        public void ResumeRun_InitializesAsEmptyList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.resumeRun);
            Assert.IsType<List<string>>(model.resumeRun);
            Assert.Empty(model.resumeRun);
        }

        [Fact]
        public void AbortRun_InitializesAsEmptyList()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new CastModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model.abortRun);
            Assert.IsType<List<string>>(model.abortRun);
            Assert.Empty(model.abortRun);
        }

        #endregion

        #region BindProperty Tests

        [Fact]
        public void SelectedValue_HasBindPropertyAttribute()
        {
            // Arrange & Act
            var propertyInfo = typeof(CastModel).GetProperty("SelectedValue");

            // Assert
            Assert.NotNull(propertyInfo);
            var attributes = propertyInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.BindPropertyAttribute), false);
            Assert.NotEmpty(attributes);
        }

        #endregion
    }
}

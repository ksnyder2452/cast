using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;
using System.Net.Mime;
using Microsoft.AspNetCore.Hosting;

namespace Execution_UI.Tests.Pages
{
    /// <summary>
    /// Unit tests for IndexModel page model.
    /// Tests page initialization and file download functionality.
    /// </summary>
    public class IndexPageTests
    {
        private Mock<ILogger<IndexModel>> CreateMockLogger()
        {
            return new Mock<ILogger<IndexModel>>();
        }

        private Mock<IWebHostEnvironment> CreateMockEnvironment(string? contentRootPath = null)
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            if (contentRootPath != null)
            {
                mockEnvironment.Setup(e => e.ContentRootPath).Returns(contentRootPath);
            }
            return mockEnvironment;
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesSuccessfully()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var mockEnvironment = CreateMockEnvironment();

            // Act
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_WithNullLogger_AcceptsNull()
        {
            // Arrange
            var mockEnvironment = CreateMockEnvironment();

            // Act & Assert - Constructor signature allows null
            var model = new IndexModel(null!, mockEnvironment.Object);
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_WithNullEnvironment_AcceptsNull()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act & Assert - Constructor signature allows null
            var model = new IndexModel(mockLogger.Object, null!);
            Assert.NotNull(model);
        }

        #endregion

        #region OnGet Tests

        [Fact]
        public void OnGet_DoesNotThrowException()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var mockEnvironment = CreateMockEnvironment();
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => model.OnGet());
            Assert.Null(exception);
        }

        [Fact]
        public void OnGet_ExecutesSuccessfully()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var mockEnvironment = CreateMockEnvironment();
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Act
            model.OnGet();

            // Assert - Model should be in valid state after OnGet
            Assert.NotNull(model);
        }

        #endregion

        #region OnGetDownloadClientDLL Tests

        [Fact]
        public void OnGetDownloadClientDLL_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_dll_notfound_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                var result = model.OnGetDownloadClientDLL();

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnGetDownloadClientDLL_FileExists_ReturnsPhysicalFile()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_dll_exists_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            var testFilePath = Path.Combine(clientDir, "CAST_Client_Service.dll");
            File.WriteAllText(testFilePath, "test dll content");

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                var result = model.OnGetDownloadClientDLL();

                // Assert
                Assert.IsType<PhysicalFileResult>(result);
                var fileResult = result as PhysicalFileResult;
                Assert.NotNull(fileResult);
                Assert.Equal("CAST_Client_Service.dll", fileResult.FileDownloadName);
                Assert.Equal(MediaTypeNames.Application.Octet, fileResult.ContentType);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnGetDownloadClientDLL_FileExists_ReturnsCorrectContentType()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_dll_contenttype_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            var testFilePath = Path.Combine(clientDir, "CAST_Client_Service.dll");
            File.WriteAllText(testFilePath, "test content");

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                var result = model.OnGetDownloadClientDLL() as PhysicalFileResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("application/octet-stream", result.ContentType);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnGetDownloadClientDLL_FileExists_ReturnsCorrectFileName()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_dll_filename_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            var testFilePath = Path.Combine(clientDir, "CAST_Client_Service.dll");
            File.WriteAllText(testFilePath, "test");

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                var result = model.OnGetDownloadClientDLL() as PhysicalFileResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("CAST_Client_Service.dll", result.FileDownloadName);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnGetDownloadClientDLL_NoClientsDirectory_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_dll_nodir_{Guid.NewGuid()}");
            // Don't create the clients directory

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                var result = model.OnGetDownloadClientDLL();

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnGetDownloadClientDLL_EnvironmentContentRootPathIsUsed()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var expectedContentRoot = "/expected/content/root";
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.ContentRootPath).Returns(expectedContentRoot);

            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Act
            var result = model.OnGetDownloadClientDLL();

            // Assert
            mockEnvironment.Verify(e => e.ContentRootPath, Times.AtLeastOnce);
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void IndexModel_WorkflowOnGetThenOnGetDownloadClientDLL()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_workflow_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            var testFilePath = Path.Combine(clientDir, "CAST_Client_Service.dll");
            File.WriteAllText(testFilePath, "content");

            try
            {
                var mockEnvironment = CreateMockEnvironment(tempDir);
                var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

                // Act
                model.OnGet();
                var downloadResult = model.OnGetDownloadClientDLL();

                // Assert
                Assert.NotNull(model);
                Assert.IsType<PhysicalFileResult>(downloadResult);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        #endregion
    }
}

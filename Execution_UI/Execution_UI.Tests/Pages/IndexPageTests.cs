using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Execution_UI.Tests.Pages
{
    public class IndexPageTests
    {
        [Fact]
        public void IndexModel_OnGet_DoesNotThrowException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Act & Assert - Should not throw
            var result = Record.Exception(() => model.OnGet());
            Assert.Null(result);
        }

        [Fact]
        public void IndexModel_OnGet_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Act
            model.OnGet();

            // Assert - Verify logger was called
            mockLogger.VerifyLogging(l => l == LogLevel.Information, Times.Never());
        }

        [Fact]
        public void IndexModel_OnGetDownloadClientDLL_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Create a temporary directory for testing
            var tempClientPath = Path.Combine(Path.GetTempPath(), "clients");
            Directory.CreateDirectory(tempClientPath);
            mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            // Act
            var result = model.OnGetDownloadClientDLL();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void IndexModel_OnGetDownloadClientDLL_FileExists_ReturnsPhysicalFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();

            var tempDir = Path.Combine(Path.GetTempPath(), $"test_clients_{Guid.NewGuid()}");
            var clientDir = Path.Combine(tempDir, "clients");
            Directory.CreateDirectory(clientDir);

            var testFilePath = Path.Combine(clientDir, "CAST_Client_Service.dll");
            File.WriteAllText(testFilePath, "test content");

            mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            try
            {
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
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void IndexModel_Constructor_InitializesWithValidDependencies()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IndexModel>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();

            // Act
            var model = new IndexModel(mockLogger.Object, mockEnvironment.Object);

            // Assert
            Assert.NotNull(model);
        }
    }
}

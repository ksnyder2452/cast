using Xunit;
using System.Text;
using FileStorageService;

namespace File_Storage_Service.Tests
{
    /// <summary>
    /// Integration tests for file storage result operations
    /// </summary>
    public class FileStorageResultTests
    {
        [Fact]
        public void FileStorageResult_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var result = new FileStorageResult();

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.PathName);
            Assert.Null(result.FileName);
            Assert.Null(result.Originator);
            Assert.Null(result.Type);
            Assert.Null(result.Message);
            Assert.Null(result.FullPath);
        }

        [Fact]
        public void FileStorageResult_CanSetAllProperties()
        {
            // Arrange
            var result = new FileStorageResult
            {
                Success = true,
                ErrorMessage = "Test error",
                PathName = "test/path/",
                FileName = "test.txt",
                Originator = "test_originator",
                Type = "INFO",
                Message = "Test message",
                FullPath = "/full/path/test.txt"
            };

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Test error", result.ErrorMessage);
            Assert.Equal("test/path/", result.PathName);
            Assert.Equal("test.txt", result.FileName);
            Assert.Equal("test_originator", result.Originator);
            Assert.Equal("INFO", result.Type);
            Assert.Equal("Test message", result.Message);
            Assert.Equal("/full/path/test.txt", result.FullPath);
        }

        [Fact]
        public void FileStorageResult_SuccessScenario_HasExpectedState()
        {
            // Arrange
            var result = new FileStorageResult
            {
                Success = true,
                PathName = "uploads/",
                FileName = "document.pdf"
            };

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.PathName);
            Assert.NotNull(result.FileName);
        }

        [Fact]
        public void FileStorageResult_FailureScenario_HasExpectedState()
        {
            // Arrange
            var result = new FileStorageResult
            {
                Success = false,
                ErrorMessage = "File not found"
            };

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }
    }
}

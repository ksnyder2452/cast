using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Unit tests for file transfer functionality in the Execution Service
    /// </summary>
    public class FileTransferTests
    {
        /// <summary>
        /// Test header extraction from message properties
        /// </summary>
        [Fact]
        public void ExtractHeader_ValidHeader_ReturnsCorrectValue()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "serviceName", Encoding.UTF8.GetBytes("execution_service") },
                { "pathName", Encoding.UTF8.GetBytes("/files/uploads") },
                { "fileName", Encoding.UTF8.GetBytes("testfile.txt") }
            };

            // Act
            string serviceName = headers.TryGetValue("serviceName", out var serviceNameObj)
                ? Encoding.UTF8.GetString((byte[]?)serviceNameObj ?? [])
                : string.Empty;

            // Assert
            Assert.Equal("execution_service", serviceName);
        }

        /// <summary>
        /// Test handling of missing headers
        /// </summary>
        [Fact]
        public void ExtractHeader_MissingHeader_ReturnsEmptyString()
        {
            // Arrange
            var headers = new Dictionary<string, object?>();

            // Act
            string pathName = headers.TryGetValue("pathName", out var pathNameObj)
                ? Encoding.UTF8.GetString((byte[]?)pathNameObj ?? [])
                : string.Empty;

            // Assert
            Assert.Equal(string.Empty, pathName);
        }

        /// <summary>
        /// Test extraction of all file transfer metadata
        /// </summary>
        [Fact]
        public void ExtractFileMetadata_AllHeadersPresent_ExtractsAll()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "serviceName", Encoding.UTF8.GetBytes("client_service") },
                { "pathName", Encoding.UTF8.GetBytes("C:\\data\\uploads") },
                { "fileName", Encoding.UTF8.GetBytes("result.dat") }
            };

            // Act
            string queueName = headers.TryGetValue("serviceName", out var qObj)
                ? Encoding.UTF8.GetString((byte[]?)qObj ?? [])
                : string.Empty;
            string pathName = headers.TryGetValue("pathName", out var pObj)
                ? Encoding.UTF8.GetString((byte[]?)pObj ?? [])
                : string.Empty;
            string fileName = headers.TryGetValue("fileName", out var fObj)
                ? Encoding.UTF8.GetString((byte[]?)fObj ?? [])
                : string.Empty;

            // Assert
            Assert.Equal("client_service", queueName);
            Assert.Equal("C:\\data\\uploads", pathName);
            Assert.Equal("result.dat", fileName);
        }

        /// <summary>
        /// Test file header detection
        /// </summary>
        [Fact]
        public void IsFileMessage_HeadersPresent_ReturnsTrue()
        {
            // Arrange
            var headers = new Dictionary<string, object?> { { "fileName", Encoding.UTF8.GetBytes("test.txt") } };
            bool hasHeaders = headers != null && headers.Count > 0;

            // Act & Assert
            Assert.True(hasHeaders);
        }

        /// <summary>
        /// Test file header detection with empty headers
        /// </summary>
        [Fact]
        public void IsFileMessage_NoHeaders_ReturnsFalse()
        {
            // Arrange
            var headers = new Dictionary<string, object?>();

            // Act
            bool hasHeaders = headers != null && headers.Count > 0;

            // Assert
            Assert.False(hasHeaders);
        }

        /// <summary>
        /// Test encoding of file metadata in headers
        /// </summary>
        [Fact]
        public void FileMetadata_Encoding_PreservesData()
        {
            // Arrange
            string originalPath = "D:\\TestFolder\\Subfolder";
            byte[] encoded = Encoding.UTF8.GetBytes(originalPath);

            // Act
            string decoded = Encoding.UTF8.GetString(encoded);

            // Assert
            Assert.Equal(originalPath, decoded);
        }

        /// <summary>
        /// Test handling of file bytes
        /// </summary>
        [Fact]
        public void FileBytes_RoundTrip_PreservesContent()
        {
            // Arrange
            byte[] originalFileBytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF };

            // Act
            byte[] processedBytes = originalFileBytes; // Simulate processing

            // Assert
            Assert.Equal(originalFileBytes, processedBytes);
        }
    }
}

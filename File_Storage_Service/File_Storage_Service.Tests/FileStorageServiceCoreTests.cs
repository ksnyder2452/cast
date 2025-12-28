using Xunit;
using Moq;
using RabbitMQ.Client;
using System.Text;
using System.IO.Compression;
using FileStorageService;

namespace File_Storage_Service.Tests
{
    /// <summary>
    /// Unit tests for FileStorageServiceCore
    /// </summary>
    public class FileStorageServiceCoreTests : IDisposable
    {
        private readonly Mock<IConnectionFactory> _mockConnectionFactory;
        private readonly string _testDirectory;
        private readonly FileStorageServiceCore _service;

        public FileStorageServiceCoreTests()
        {
            _mockConnectionFactory = new Mock<IConnectionFactory>();
            _testDirectory = Path.Combine(Path.GetTempPath(), "test_file_storage_" + Guid.NewGuid().ToString());
            _service = new FileStorageServiceCore(_mockConnectionFactory.Object, "test_service", _testDirectory);
        }

        public void Dispose()
        {
            // Cleanup test directory
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullConnectionFactory_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FileStorageServiceCore(null!, "test_service"));
        }

        [Fact]
        public void Constructor_WithNullServiceName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FileStorageServiceCore(_mockConnectionFactory.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange & Act
            var service = new FileStorageServiceCore(_mockConnectionFactory.Object, "test_service");

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region Directory Initialization Tests

        [Fact]
        public void InitializeDirectoryStructure_CreatesRequiredDirectories()
        {
            // Act
            _service.InitializeDirectoryStructure();

            // Assert
            Assert.True(Directory.Exists(Path.Combine(_testDirectory, "inbound_queue")));
            Assert.True(Directory.Exists(Path.Combine(_testDirectory, "outbound_queue")));
            Assert.True(Directory.Exists(Path.Combine(_testDirectory, "working_queue")));
        }

        [Fact]
        public void InitializeDirectoryStructure_CalledMultipleTimes_DoesNotThrow()
        {
            // Act & Assert
            _service.InitializeDirectoryStructure();
            _service.InitializeDirectoryStructure(); // Should not throw

            Assert.True(Directory.Exists(Path.Combine(_testDirectory, "inbound_queue")));
        }

        #endregion

        #region Log Message Creation Tests

        [Fact]
        public void CreateStartupLogMessage_ReturnsCorrectSqlStatement()
        {
            // Arrange
            var uuid = Guid.NewGuid().ToString();
            var referenceUuid = Guid.NewGuid().ToString();

            // Act
            var result = _service.CreateStartupLogMessage(uuid, referenceUuid);

            // Assert
            Assert.Contains("insert into logger", result);
            Assert.Contains(uuid, result);
            Assert.Contains(referenceUuid, result);
            Assert.Contains("file_storage_service", result);
            Assert.Contains("INFO", result);
            Assert.Contains("Started test_service", result);
        }

        [Fact]
        public void CreateShutdownLogMessage_ReturnsCorrectSqlStatement()
        {
            // Arrange
            var uuid = Guid.NewGuid().ToString();
            var referenceUuid = Guid.NewGuid().ToString();

            // Act
            var result = _service.CreateShutdownLogMessage(uuid, referenceUuid);

            // Assert
            Assert.Contains("insert into logger", result);
            Assert.Contains(uuid, result);
            Assert.Contains(referenceUuid, result);
            Assert.Contains("Stopped test_service", result);
        }

        [Fact]
        public void CreateFileReceivedLogMessage_ReturnsCorrectSqlStatement()
        {
            // Arrange
            var uuid = Guid.NewGuid().ToString();
            var referenceUuid = Guid.NewGuid().ToString();
            var originator = "test_originator";
            var type = "INFO";
            var message = "Test message";

            // Act
            var result = _service.CreateFileReceivedLogMessage(uuid, referenceUuid, originator, type, message);

            // Assert
            Assert.Contains("insert into logger", result);
            Assert.Contains(uuid, result);
            Assert.Contains(referenceUuid, result);
            Assert.Contains(originator, result);
            Assert.Contains(type, result);
            Assert.Contains(message, result);
        }

        #endregion

        #region State Tracker Message Creation Tests

        [Fact]
        public void CreateStateRegistrationDeleteMessage_ReturnsCorrectSqlStatement()
        {
            // Act
            var result = _service.CreateStateRegistrationDeleteMessage();

            // Assert
            Assert.Contains("delete ignore from cast_state_tracker", result);
            Assert.Contains("test_service", result);
        }

        [Fact]
        public void CreateStateRegistrationInsertMessage_ReturnsCorrectSqlStatement()
        {
            // Arrange
            var state = "ONLINE";

            // Act
            var result = _service.CreateStateRegistrationInsertMessage(state);

            // Assert
            Assert.Contains("insert into cast_state_tracker", result);
            Assert.Contains("test_service", result);
            Assert.Contains(state, result);
        }

        [Fact]
        public void CreateStateUpdateMessage_ReturnsCorrectSqlStatement()
        {
            // Arrange
            var state = "OFFLINE";

            // Act
            var result = _service.CreateStateUpdateMessage(state);

            // Assert
            Assert.Contains("update cast_state_tracker", result);
            Assert.Contains("test_service", result);
            Assert.Contains(state, result);
        }

        #endregion

        #region File Processing Tests

        [Fact]
        public void ProcessFileStorage_WithNullHeaders_ReturnsFailure()
        {
            // Arrange
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(null, fileData);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Headers are null", result.ErrorMessage);
        }

        [Fact]
        public void ProcessFileStorage_WithMissingPathName_ReturnsFailure()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "fileName", Encoding.UTF8.GetBytes("test.txt") }
            };
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("PathName or FileName is missing", result.ErrorMessage);
        }

        [Fact]
        public void ProcessFileStorage_WithMissingFileName_ReturnsFailure()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "pathName", Encoding.UTF8.GetBytes("test/path") }
            };
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("PathName or FileName is missing", result.ErrorMessage);
        }

        [Fact]
        public void ProcessFileStorage_WithValidHeaders_CreatesFile()
        {
            // Arrange
            _service.InitializeDirectoryStructure();
            var headers = new Dictionary<string, object?>
            {
                { "pathName", Encoding.UTF8.GetBytes("test/path") },
                { "fileName", Encoding.UTF8.GetBytes("test.txt") },
                { "originator", Encoding.UTF8.GetBytes("test_originator") },
                { "type", Encoding.UTF8.GetBytes("INFO") },
                { "message", Encoding.UTF8.GetBytes("Test message") }
            };
            var fileData = Encoding.UTF8.GetBytes("test file content");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("test/path" + Path.DirectorySeparatorChar, result.PathName);
            Assert.Equal("test.txt", result.FileName);
            Assert.Equal("test_originator", result.Originator);
            Assert.Equal("INFO", result.Type);
            Assert.Equal("Test message", result.Message);
            Assert.NotNull(result.FullPath);
            Assert.True(File.Exists(result.FullPath));

            // Verify file content
            var savedContent = File.ReadAllBytes(result.FullPath);
            Assert.Equal(fileData, savedContent);
        }

        [Fact]
        public void ProcessFileStorage_WithPathNameMissingSeparator_AddsDirectorySeparator()
        {
            // Arrange
            _service.InitializeDirectoryStructure();
            var headers = new Dictionary<string, object?>
            {
                { "pathName", Encoding.UTF8.GetBytes("test/path") },
                { "fileName", Encoding.UTF8.GetBytes("test.txt") }
            };
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.True(result.Success);
            Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result.PathName);
        }

        [Fact]
        public void ProcessFileStorage_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "pathName", Encoding.UTF8.GetBytes("new/nested/path") },
                { "fileName", Encoding.UTF8.GetBytes("test.txt") }
            };
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(result.FullPath!));
        }

        [Fact]
        public void ProcessFileStorage_WithZipFile_ExtractsAndDeletesZip()
        {
            // Arrange
            _service.InitializeDirectoryStructure();

            // Create a simple zip file with a single text file
            var tempZipDir = Path.Combine(Path.GetTempPath(), "zip_source_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempZipDir);
            var testFile = Path.Combine(tempZipDir, "testfile.txt");
            File.WriteAllText(testFile, "test content");

            var zipFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".zip");

            try
            {
                // Create zip and immediately read it, avoiding the whole temp directory
                using (var zip = System.IO.Compression.ZipFile.Open(zipFilePath, System.IO.Compression.ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(testFile, "testfile.txt");
                }

                var zipData = File.ReadAllBytes(zipFilePath);

                var headers = new Dictionary<string, object?>
                {
                    { "pathName", Encoding.UTF8.GetBytes("zip/test") },
                    { "fileName", Encoding.UTF8.GetBytes("archive.zip") }
                };

                // Act
                var result = _service.ProcessFileStorage(headers, zipData);

                // Assert
                Assert.True(result.Success);
                // The zip file should be deleted after extraction
                Assert.False(File.Exists(result.FullPath!));
                // The extracted file should exist
                var extractedFile = Path.Combine(Path.GetDirectoryName(result.FullPath)!, "testfile.txt");
                Assert.True(File.Exists(extractedFile));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempZipDir))
                {
                    Directory.Delete(tempZipDir, true);
                }
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        [Fact]
        public void ProcessFileStorage_WithEmptyHeaders_ReturnsFailure()
        {
            // Arrange
            var headers = new Dictionary<string, object?>();
            var fileData = Encoding.UTF8.GetBytes("test data");

            // Act
            var result = _service.ProcessFileStorage(headers, fileData);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetRootDirectory Tests

        [Fact]
        public void GetRootDirectory_ReturnsConfiguredDirectory()
        {
            // Act
            var result = _service.GetRootDirectory();

            // Assert
            Assert.Equal(_testDirectory, result);
        }

        #endregion
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FileStorageService
{
    /// <summary>
    /// Core service class for handling file storage operations
    /// </summary>
    public class FileStorageServiceCore
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _serviceName;
        private readonly string _rootDirectory;

        public FileStorageServiceCore(IConnectionFactory connectionFactory, string serviceName, string? rootDirectory = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _rootDirectory = rootDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "temp");
        }

        /// <summary>
        /// Initializes the local directory structure
        /// </summary>
        public void InitializeDirectoryStructure()
        {
            string rootDir = _rootDirectory + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(Path.Combine(rootDir, "inbound_queue"));
            Directory.CreateDirectory(Path.Combine(rootDir, "outbound_queue"));
            Directory.CreateDirectory(Path.Combine(rootDir, "working_queue"));
        }

        /// <summary>
        /// Creates a startup log message
        /// </summary>
        public string CreateStartupLogMessage(string uuid, string referenceUuid)
        {
            return $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('{uuid}', '{referenceUuid}', 'file_storage_service', 'INFO', 'Started {_serviceName}', NOW(), '{_serviceName}')";
        }

        /// <summary>
        /// Creates a state registration delete message
        /// </summary>
        public string CreateStateRegistrationDeleteMessage()
        {
            return $"delete ignore from cast_state_tracker where name = '{_serviceName}'";
        }

        /// <summary>
        /// Creates a state registration insert message
        /// </summary>
        public string CreateStateRegistrationInsertMessage(string state)
        {
            return $"insert into cast_state_tracker (name, state, event_time_dt) values('{_serviceName}', '{state}', NOW())";
        }

        /// <summary>
        /// Creates a state update message
        /// </summary>
        public string CreateStateUpdateMessage(string state)
        {
            return $"update cast_state_tracker set state = '{state}', event_time_dt = NOW() where name = '{_serviceName}'";
        }

        /// <summary>
        /// Creates a shutdown log message
        /// </summary>
        public string CreateShutdownLogMessage(string uuid, string referenceUuid)
        {
            return $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('{uuid}', '{referenceUuid}', 'file_storage_service', 'INFO', 'Stopped {_serviceName}', NOW())";
        }

        /// <summary>
        /// Creates a file received log message
        /// </summary>
        public string CreateFileReceivedLogMessage(string uuid, string referenceUuid, string originator, string type, string message)
        {
            return $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('{uuid}', '{referenceUuid}', '{originator}', '{type}', '{message}', NOW())";
        }

        /// <summary>
        /// Processes file storage based on headers
        /// </summary>
        public FileStorageResult ProcessFileStorage(IDictionary<string, object?>? headers, byte[] fileData)
        {
            if (headers == null)
            {
                return new FileStorageResult { Success = false, ErrorMessage = "Headers are null" };
            }

            var pathName = ExtractHeaderValue(headers, "pathName");
            var fileName = ExtractHeaderValue(headers, "fileName");
            var originator = ExtractHeaderValue(headers, "originator");
            var type = ExtractHeaderValue(headers, "type");
            var message = ExtractHeaderValue(headers, "message");

            if (string.IsNullOrEmpty(pathName) || string.IsNullOrEmpty(fileName))
            {
                return new FileStorageResult { Success = false, ErrorMessage = "PathName or FileName is missing" };
            }

            if (!pathName.EndsWith(Path.DirectorySeparatorChar))
            {
                pathName += Path.DirectorySeparatorChar;
            }

            string rootDir = _rootDirectory + Path.DirectorySeparatorChar;
            string fullPath = Path.Combine(rootDir, "inbound_queue", pathName);
            string fullFilePath = Path.Combine(fullPath, fileName);

            try
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                File.WriteAllBytes(fullFilePath, fileData);

                // If the file is zipped, unzip it
                if (fileName.EndsWith(".zip"))
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(fullFilePath, fullPath, true);
                    File.Delete(fullFilePath);
                }

                return new FileStorageResult
                {
                    Success = true,
                    PathName = pathName,
                    FileName = fileName,
                    Originator = originator,
                    Type = type,
                    Message = message,
                    FullPath = fullFilePath
                };
            }
            catch (Exception ex)
            {
                return new FileStorageResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Extracts a header value as a string
        /// </summary>
        private string ExtractHeaderValue(IDictionary<string, object?> headers, string key)
        {
            if (headers.TryGetValue(key, out var value) && value is byte[] byteArray)
            {
                return Encoding.UTF8.GetString(byteArray);
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the root directory
        /// </summary>
        public string GetRootDirectory() => _rootDirectory;
    }

    /// <summary>
    /// Result of a file storage operation
    /// </summary>
    public class FileStorageResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? PathName { get; set; }
        public string? FileName { get; set; }
        public string? Originator { get; set; }
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? FullPath { get; set; }
    }
}

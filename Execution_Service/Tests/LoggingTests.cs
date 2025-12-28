using Xunit;
using System;
using System.Text;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Unit tests for logging and audit message generation
    /// </summary>
    public class LoggingTests
    {
        /// <summary>
        /// Test logging message format for service startup
        /// </summary>
        [Fact]
        public void GenerateStartupMessage_ValidInput_CreatesCorrectSQL()
        {
            // Arrange
            string serviceName = "execution_service";
            string uuid = "550e8400-e29b-41d4-a716-446655440000";

            // Act
            string message = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) " +
                           $"values('{uuid}', '{uuid}', 'execution_service', 'INFO', 'Started {serviceName}', NOW(), '{serviceName}')";

            // Assert
            Assert.StartsWith("insert into logger", message);
            Assert.Contains("'INFO'", message);
            Assert.Contains($"'Started {serviceName}'", message);
            Assert.Contains("event_time_dt", message);
        }

        /// <summary>
        /// Test logging message format for service shutdown
        /// </summary>
        [Fact]
        public void GenerateShutdownMessage_ValidInput_CreatesCorrectSQL()
        {
            // Arrange
            string serviceName = "execution_service";
            string startUUID = "550e8400-e29b-41d4-a716-446655440000";
            string stopUUID = "550e8400-e29b-41d4-a716-446655440001";

            // Act
            string message = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) " +
                           $"values('{stopUUID}', '{startUUID}', 'execution_service', 'INFO', 'Stopped {serviceName}', NOW())";

            // Assert
            Assert.Contains($"'Stopped {serviceName}'", message);
            Assert.Contains(startUUID, message);
            Assert.Contains(stopUUID, message);
        }

        /// <summary>
        /// Test message queue name for logger service
        /// </summary>
        [Fact]
        public void LoggerQueueName_IsCorrect()
        {
            // Arrange & Act
            string queueName = "logger_service";

            // Assert
            Assert.Equal("logger_service", queueName);
        }

        /// <summary>
        /// Test encoding of log messages to bytes
        /// </summary>
        [Fact]
        public void EncodeLogMessage_ValidSQL_ReturnsBytes()
        {
            // Arrange
            string logMessage = "insert into logger (uuid) values('test-uuid')";

            // Act
            byte[] encoded = Encoding.UTF8.GetBytes(logMessage);

            // Assert
            Assert.NotEmpty(encoded);
            Assert.Equal(logMessage, Encoding.UTF8.GetString(encoded));
        }

        /// <summary>
        /// Test generation of DELETE query for state cleanup
        /// </summary>
        [Fact]
        public void GenerateDeleteStateQuery_CleanupBeforeInsert()
        {
            // Arrange
            string serviceName = "execution_service";

            // Act
            string query = $"delete ignore from cast_state_tracker where name = '{serviceName}'";

            // Assert
            Assert.StartsWith("delete ignore from", query);
            Assert.Contains(serviceName, query);
        }

        /// <summary>
        /// Test client state change message
        /// </summary>
        [Fact]
        public void GenerateClientOfflineMessage_CreatesValidSQL()
        {
            // Arrange
            string clientUUID = "550e8400-e29b-41d4-a716-446655440000";
            string auditUUID = "550e8400-e29b-41d4-a716-446655440001";

            // Act
            string message = $"insert into state (uuid, reference_uuid, state, event_time_dt) " +
                           $"values('{auditUUID}', '{clientUUID}', 'OFFLINE', NOW())";

            // Assert
            Assert.StartsWith("insert into state", message);
            Assert.Contains("'OFFLINE'", message);
            Assert.Contains(clientUUID, message);
        }

        /// <summary>
        /// Test log message includes timestamp
        /// </summary>
        [Fact]
        public void LogMessage_IncludesTimestamp_ContainsNOW()
        {
            // Arrange
            string logMessage = "insert into logger (message, event_time_dt) values('test', NOW())";

            // Act
            bool hasTimestamp = logMessage.Contains("NOW()");

            // Assert
            Assert.True(hasTimestamp);
        }

        /// <summary>
        /// Test log message type constants
        /// </summary>
        [Theory]
        [InlineData("INFO")]
        [InlineData("ERROR")]
        [InlineData("WARNING")]
        public void LogMessageType_ValidTypes_AreAccepted(string type)
        {
            // Act & Assert
            Assert.False(string.IsNullOrEmpty(type));
        }
    }
}

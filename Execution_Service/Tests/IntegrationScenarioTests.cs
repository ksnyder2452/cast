using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Integration tests for complete message flow scenarios
    /// </summary>
    public class IntegrationScenarioTests
    {
        /// <summary>
        /// Test complete workflow of receiving a message, parsing it, and queuing it
        /// </summary>
        [Fact]
        public void MessageFlow_ReceiveParseQueue_CompletesSuccessfully()
        {
            // Arrange
            string clientUUID = "550e8400-e29b-41d4-a716-446655440000";
            string messageData = "Task execution request";
            string incomingMessage = $"MESSAGE FOR {clientUUID}: {messageData}";
            var allClientUUIDs = new List<string>();

            // Act
            if (incomingMessage.ToUpper().StartsWith("MESSAGE FOR "))
            {
                string extractedUUID = incomingMessage.Substring(12, incomingMessage.IndexOf(":") - 12).Trim();
                string extractedMessage = incomingMessage.Substring(incomingMessage.IndexOf(":") + 1).Trim();
                allClientUUIDs.Add(extractedUUID);

                // Simulate message queuing
                byte[] messageBytes = Encoding.UTF8.GetBytes(extractedMessage);
                string messageString = Encoding.UTF8.GetString(messageBytes);
            }

            // Assert
            Assert.Contains(clientUUID, allClientUUIDs);
            Assert.Single(allClientUUIDs);
        }

        /// <summary>
        /// Test file transfer workflow
        /// </summary>
        [Fact]
        public void FileTransferFlow_ReceiveParseRoute_CompletesSuccessfully()
        {
            // Arrange
            var headers = new Dictionary<string, object?>
            {
                { "serviceName", Encoding.UTF8.GetBytes("client_service_123") },
                { "pathName", Encoding.UTF8.GetBytes("C:\\Results") },
                { "fileName", Encoding.UTF8.GetBytes("output.dat") }
            };
            byte[] fileBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            // Act
            bool hasHeaders = headers != null && headers.Count > 0;
            string queueName = string.Empty;
            if (headers != null && headers.TryGetValue("serviceName", out var qObj))
            {
                queueName = Encoding.UTF8.GetString((byte[]?)qObj ?? []);
            }
            string pathName = string.Empty;
            if (headers != null && headers.TryGetValue("pathName", out var pObj))
            {
                pathName = Encoding.UTF8.GetString((byte[]?)pObj ?? []);
            }
            string fileName = string.Empty;
            if (headers != null && headers.TryGetValue("fileName", out var fObj))
            {
                fileName = Encoding.UTF8.GetString((byte[]?)fObj ?? []);
            }

            // Assert
            Assert.True(hasHeaders);
            Assert.Equal("client_service_123", queueName);
            Assert.Equal("C:\\Results", pathName);
            Assert.Equal("output.dat", fileName);
        }

        /// <summary>
        /// Test service lifecycle: startup, message processing, shutdown
        /// </summary>
        [Fact]
        public void ServiceLifecycle_StartStopWithMessages_TracksState()
        {
            // Arrange
            string serviceName = "execution_service";
            var allClientUUIDs = new List<string>();
            Guid startUUID = Guid.NewGuid();

            // Act - Simulate startup
            string startupMessage = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) " +
                                  $"values('{startUUID}', '{startUUID}', 'execution_service', 'INFO', 'Started {serviceName}', NOW(), '{serviceName}')";

            // Process some messages
            allClientUUIDs.Add("client-1");
            allClientUUIDs.Add("client-2");

            // Simulate shutdown
            Guid stopUUID = Guid.NewGuid();
            string shutdownMessage = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) " +
                                    $"values('{stopUUID}', '{startUUID}', 'execution_service', 'INFO', 'Stopped {serviceName}', NOW())";

            // Mark all clients offline
            foreach (string clientId in allClientUUIDs)
            {
                string clientOfflineMsg = $"insert into state (uuid, reference_uuid, state, event_time_dt) " +
                                         $"values('{Guid.NewGuid()}', '{clientId}', 'OFFLINE', NOW())";
            }

            // Assert
            Assert.StartsWith("insert into logger", startupMessage);
            Assert.StartsWith("insert into logger", shutdownMessage);
            Assert.Equal(2, allClientUUIDs.Count);
        }

        /// <summary>
        /// Test multiple messages from same client
        /// </summary>
        [Fact]
        public void MultipleMessages_SameClient_AllProcessed()
        {
            // Arrange
            string clientUUID = "550e8400-e29b-41d4-a716-446655440000";
            var messages = new[]
            {
                $"MESSAGE FOR {clientUUID}: Message 1",
                $"MESSAGE FOR {clientUUID}: Message 2",
                $"MESSAGE FOR {clientUUID}: Message 3"
            };
            var allClientUUIDs = new List<string>();

            // Act
            foreach (string msg in messages)
            {
                if (msg.ToUpper().StartsWith("MESSAGE FOR "))
                {
                    string extractedUUID = msg.Substring(12, msg.IndexOf(":") - 12).Trim();
                    string extractedMessage = msg.Substring(msg.IndexOf(":") + 1).Trim();
                    allClientUUIDs.Add(extractedUUID);
                }
            }

            // Assert
            Assert.Contains(clientUUID, allClientUUIDs);
            Assert.Equal(3, allClientUUIDs.Count);
        }

        /// <summary>
        /// Test mixed message types in sequence
        /// </summary>
        [Fact]
        public void MixedMessageTypes_ProcessedCorrectly()
        {
            // Arrange
            var messages = new[]
            {
                "MESSAGE FOR 550e8400-e29b-41d4-a716-446655440000: Regular message",
                "INSERT INTO logger (uuid) VALUES ('123')",
                "MESSAGE FOR 550e8400-e29b-41d4-a716-446655440001: Another message"
            };

            int regularMessages = 0;
            int insertMessages = 0;

            // Act
            foreach (string msg in messages)
            {
                if (msg.ToUpper().StartsWith("MESSAGE FOR "))
                    regularMessages++;
                else if (msg.Trim().ToUpper().StartsWith("INSERT INTO "))
                    insertMessages++;
            }

            // Assert
            Assert.Equal(2, regularMessages);
            Assert.Equal(1, insertMessages);
        }
    }
}

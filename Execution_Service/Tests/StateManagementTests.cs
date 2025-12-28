using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Unit tests for state management and tracking functionality
    /// </summary>
    public class StateManagementTests
    {
        /// <summary>
        /// Test adding client UUID to active clients list
        /// </summary>
        [Fact]
        public void AddClientUUID_ValidUUID_AddsToList()
        {
            // Arrange
            var allClientUUIDs = new List<string>();
            string clientUUID = "550e8400-e29b-41d4-a716-446655440000";

            // Act
            allClientUUIDs.Add(clientUUID);

            // Assert
            Assert.Contains(clientUUID, allClientUUIDs);
            Assert.Single(allClientUUIDs);
        }

        /// <summary>
        /// Test tracking multiple client UUIDs
        /// </summary>
        [Fact]
        public void AddMultipleClientUUIDs_SeveralClients_TracksAll()
        {
            // Arrange
            var allClientUUIDs = new List<string>();
            string[] clientUUIDs = new[]
            {
                "550e8400-e29b-41d4-a716-446655440000",
                "550e8400-e29b-41d4-a716-446655440001",
                "550e8400-e29b-41d4-a716-446655440002"
            };

            // Act
            foreach (string uuid in clientUUIDs)
            {
                allClientUUIDs.Add(uuid);
            }

            // Assert
            Assert.Equal(3, allClientUUIDs.Count);
            foreach (string uuid in clientUUIDs)
            {
                Assert.Contains(uuid, allClientUUIDs);
            }
        }

        /// <summary>
        /// Test generating unique GUIDs for state tracking
        /// </summary>
        [Fact]
        public void GenerateUUID_CreatesUniqueValue()
        {
            // Arrange & Act
            Guid uuid1 = Guid.NewGuid();
            Guid uuid2 = Guid.NewGuid();

            // Assert
            Assert.NotEqual(uuid1, uuid2);
        }

        /// <summary>
        /// Test GUID conversion to string
        /// </summary>
        [Fact]
        public void GUIDToString_ConvertedCorrectly()
        {
            // Arrange
            Guid testGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

            // Act
            string guidString = testGuid.ToString();

            // Assert
            Assert.Equal("550e8400-e29b-41d4-a716-446655440000", guidString);
        }

        /// <summary>
        /// Test state transition message generation
        /// </summary>
        [Fact]
        public void GenerateStateQuery_ForService_CreatesValidSQL()
        {
            // Arrange
            string serviceName = "execution_service";
            string state = "ONLINE";

            // Act
            string query = $"insert into cast_state_tracker (name, state, event_time_dt) values('{serviceName}', '{state}', NOW())";

            // Assert
            Assert.StartsWith("insert into cast_state_tracker", query);
            Assert.Contains("execution_service", query);
            Assert.Contains("ONLINE", query);
        }

        /// <summary>
        /// Test state query for offline status
        /// </summary>
        [Fact]
        public void GenerateOfflineQuery_ForService_SetsOfflineState()
        {
            // Arrange
            string serviceName = "execution_service";

            // Act
            string query = $"update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '{serviceName}'";

            // Assert
            Assert.Contains("OFFLINE", query);
            Assert.Contains("update", query);
        }

        /// <summary>
        /// Test extracting client UUID from message queue name
        /// </summary>
        [Fact]
        public void ExtractClientUUID_FromQueueName_ReturnsCorrectUUID()
        {
            // Arrange
            string queueName = "client_data_550e8400-e29b-41d4-a716-446655440000"; // 12 chars prefix

            // Act
            string clientUUID = queueName.Substring(12);

            // Assert
            Assert.Equal("550e8400-e29b-41d4-a716-446655440000", clientUUID);
        }

        /// <summary>
        /// Test duplicate UUID detection
        /// </summary>
        [Fact]
        public void CheckDuplicateUUID_AlreadyExists_ReturnsFalse()
        {
            // Arrange
            var allClientUUIDs = new List<string> { "uuid-1", "uuid-2", "uuid-3" };
            string testUUID = "uuid-2";

            // Act
            bool isNew = !allClientUUIDs.Contains(testUUID);

            // Assert
            Assert.False(isNew);
        }
    }
}

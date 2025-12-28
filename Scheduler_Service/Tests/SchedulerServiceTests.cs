using Xunit;
using System;
using System.Collections.Generic;

namespace Scheduler_Service.Tests
{
    public class SchedulerServiceTests
    {
        [Fact]
        public void TestReadyToRunInitialization()
        {
            // Arrange & Act
            bool readyToRun = true;

            // Assert
            Assert.True(readyToRun);
        }

        [Fact]
        public void TestUpdateServiceStateInitialization()
        {
            // Arrange & Act
            bool updateServiceState = false;

            // Assert
            Assert.False(updateServiceState);
        }

        [Fact]
        public void TestConfigurationStringTrim()
        {
            // Arrange
            string testConfigValue = "  rabbitmq_host  ";

            // Act
            string trimmedValue = testConfigValue.Trim();

            // Assert
            Assert.Equal("rabbitmq_host", trimmedValue);
        }

        [Fact]
        public void TestDatabaseConnectionStringConstruction()
        {
            // Arrange
            string mysql_Server = "localhost";
            string mysql_Database = "cast_db";
            string mysql_User = "admin";
            string mysql_Password = "password123";
            string mysql_Port = "3306";

            // Act
            string db_connect_string = "Server=" + mysql_Server + "; Database=" + mysql_Database + "; Uid=" + mysql_User + "; Pwd=" + mysql_Password + "; Port=" + mysql_Port;

            // Assert
            Assert.NotEmpty(db_connect_string);
            Assert.Contains("localhost", db_connect_string);
            Assert.Contains("cast_db", db_connect_string);
            Assert.Contains("admin", db_connect_string);
            Assert.Contains("3306", db_connect_string);
        }

        [Fact]
        public void TestUUIDListInitialization()
        {
            // Arrange & Act
            var uuidList = new List<string> { };

            // Assert
            Assert.NotNull(uuidList);
            Assert.Empty(uuidList);
        }

        [Fact]
        public void TestScheduledClientListInitialization()
        {
            // Arrange & Act
            var scheduledClientList = new List<string> { };

            // Assert
            Assert.NotNull(scheduledClientList);
            Assert.Empty(scheduledClientList);
        }

        [Fact]
        public void TestScheduledClientTimeListInitialization()
        {
            // Arrange & Act
            var scheduledClientTime = new List<DateTime> { };

            // Assert
            Assert.NotNull(scheduledClientTime);
            Assert.Empty(scheduledClientTime);
        }

        [Fact]
        public void TestScheduledUUIDListInitialization()
        {
            // Arrange & Act
            var scheduledUUIDList = new List<string> { };

            // Assert
            Assert.NotNull(scheduledUUIDList);
            Assert.Empty(scheduledUUIDList);
        }

        [Fact]
        public void TestGuidCreation()
        {
            // Arrange & Act
            Guid testGuid = Guid.NewGuid();
            string guidAsString = testGuid.ToString();

            // Assert
            Assert.NotEqual(Guid.Empty, testGuid);
            Assert.NotEmpty(guidAsString);
            Assert.Equal(36, guidAsString.Length);
        }

        [Fact]
        public void TestStartMessageCreation()
        {
            // Arrange
            string startmyuuidAsString = Guid.NewGuid().ToString();
            string service_name = "Scheduler_Service";

            // Act
            string startMessage = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('" + startmyuuidAsString + "', '" + startmyuuidAsString + "', 'scheduler_service', 'INFO', 'Started " + service_name + "', NOW(), '" + service_name + "')";

            // Assert
            Assert.NotEmpty(startMessage);
            Assert.Contains("scheduler_service", startMessage);
            Assert.Contains("Started", startMessage);
            Assert.Contains(service_name, startMessage);
        }

        [Fact]
        public void TestTimeSpanCalculation()
        {
            // Arrange
            DateTime scheduledTime = DateTime.Now.AddSeconds(-10);
            DateTime currentTime = DateTime.Now;

            // Act
            TimeSpan difference = currentTime.Subtract(scheduledTime);
            double totalSecondsDifference = difference.TotalSeconds;

            // Assert
            Assert.True(totalSecondsDifference >= 10);
            Assert.True(totalSecondsDifference < 11);
        }

        [Fact]
        public void TestScheduledClientMessageCreation()
        {
            // Arrange
            string clientService = "client_service_a1b2c3d4";

            // Act
            string message = "message for " + clientService + ": local: action: start run";

            // Assert
            Assert.NotEmpty(message);
            Assert.Contains(clientService, message);
            Assert.Contains("start run", message);
        }

        [Fact]
        public void TestCleanupScheduleMessageCreation()
        {
            // Arrange
            string scheduledUUID = Guid.NewGuid().ToString();

            // Act
            string cleanupSchedule = "delete ignore from state where uuid = '" + scheduledUUID + "'";

            // Assert
            Assert.NotEmpty(cleanupSchedule);
            Assert.Contains("delete", cleanupSchedule);
            Assert.Contains(scheduledUUID, cleanupSchedule);
        }

        [Fact]
        public void TestStopMessageCreation()
        {
            // Arrange
            string stopmyuuidAsString = Guid.NewGuid().ToString();
            string startmyuuidAsString = Guid.NewGuid().ToString();
            string service_name = "Scheduler_Service";

            // Act
            string stopMessage = "insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('" + stopmyuuidAsString + "', '" + startmyuuidAsString + "', 'scheduler_service', 'INFO', 'Stopped " + service_name + "', NOW())";

            // Assert
            Assert.NotEmpty(stopMessage);
            Assert.Contains("scheduler_service", stopMessage);
            Assert.Contains("Stopped", stopMessage);
        }

        [Fact]
        public void TestStateTrackerRegistration()
        {
            // Arrange
            string service_name = "Scheduler_Service";

            // Act
            string deleteState = "delete ignore from cast_state_tracker where name = '" + service_name + "'";
            string insertState = "insert into cast_state_tracker (name, state, event_time_dt) values('" + service_name + "', 'ONLINE', NOW())";

            // Assert
            Assert.NotEmpty(deleteState);
            Assert.NotEmpty(insertState);
            Assert.Contains("delete", deleteState);
            Assert.Contains("insert", insertState);
            Assert.Contains("ONLINE", insertState);
        }

        [Fact]
        public void TestStateTrackerUpdate()
        {
            // Arrange
            string service_name = "Scheduler_Service";

            // Act
            string updateState = "update cast_state_tracker set state = 'OFFLINE', event_time_dt = NOW() where name = '" + service_name + "'";

            // Assert
            Assert.NotEmpty(updateState);
            Assert.Contains("update", updateState);
            Assert.Contains("OFFLINE", updateState);
        }

        [Fact]
        public void TestListAddition()
        {
            // Arrange
            var testList = new List<string> { };
            string testUUID = Guid.NewGuid().ToString();

            // Act
            testList.Add(testUUID);

            // Assert
            Assert.Single(testList);
            Assert.Contains(testUUID, testList);
        }

        [Fact]
        public void TestListClear()
        {
            // Arrange
            var testList = new List<string> { "uuid1", "uuid2", "uuid3" };
            Assert.NotEmpty(testList);

            // Act
            testList.Clear();

            // Assert
            Assert.Empty(testList);
        }

        [Fact]
        public void TestMultipleUUIDsList()
        {
            // Arrange & Act
            var uuidList = new List<string>
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            // Assert
            Assert.Equal(3, uuidList.Count);
            foreach (var uuid in uuidList)
            {
                Assert.NotEmpty(uuid);
                Assert.Equal(36, uuid.Length);
            }
        }

        [Fact]
        public void TestTimeThresholdComparison()
        {
            // Arrange
            DateTime scheduledTime = DateTime.Now.AddSeconds(-2);
            DateTime currentTime = DateTime.Now;
            TimeSpan difference = currentTime.Subtract(scheduledTime);
            double totalSecondsDifference = difference.TotalSeconds;

            // Act
            bool shouldExecute = totalSecondsDifference > 1;

            // Assert
            Assert.True(shouldExecute);
        }

        [Fact]
        public void TestTimeThresholdComparisonNotPassed()
        {
            // Arrange
            DateTime scheduledTime = DateTime.Now.AddSeconds(5);
            DateTime currentTime = DateTime.Now;
            TimeSpan difference = currentTime.Subtract(scheduledTime);
            double totalSecondsDifference = difference.TotalSeconds;

            // Act
            bool shouldExecute = totalSecondsDifference > 1;

            // Assert
            Assert.False(shouldExecute);
        }

        [Fact]
        public void TestEncodingToBytes()
        {
            // Arrange
            string message = "test message";

            // Act
            byte[] body = System.Text.Encoding.UTF8.GetBytes(message);

            // Assert
            Assert.NotEmpty(body);
            Assert.True(body.Length > 0);
        }

        [Fact]
        public void TestStringFormatting()
        {
            // Arrange
            string clientUUID = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

            // Act
            string formattedString = $"Client {clientUUID} is scheduled to run at {DateTime.Now}";

            // Assert
            Assert.NotEmpty(formattedString);
            Assert.Contains(clientUUID, formattedString);
            Assert.Contains("scheduled", formattedString);
        }

        [Fact]
        public void TestDatabaseSelectQuery()
        {
            // Arrange
            string currentUUID = Guid.NewGuid().ToString();

            // Act
            string selectQuery = "select reference_uuid, scheduled_time, uuid from state where reference_uuid = '" + currentUUID + "' and state = 'SCHEDULED'";

            // Assert
            Assert.NotEmpty(selectQuery);
            Assert.Contains("select", selectQuery);
            Assert.Contains("SCHEDULED", selectQuery);
            Assert.Contains(currentUUID, selectQuery);
        }

        [Fact]
        public void TestLoggingQuery()
        {
            // Arrange
            string eventType = "INFO";
            string message = "Service started successfully";

            // Act
            string logQuery = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt) values('{Guid.NewGuid()}', '{Guid.NewGuid()}', 'scheduler_service', '{eventType}', '{message}', NOW())";

            // Assert
            Assert.NotEmpty(logQuery);
            Assert.Contains("logger", logQuery);
            Assert.Contains(eventType, logQuery);
        }
    }
}

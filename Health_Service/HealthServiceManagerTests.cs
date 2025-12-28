using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Unit tests for HealthServiceManager class
/// </summary>
public class HealthServiceManagerTests
{
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IDatabaseConnector> _mockDatabaseConnector;
    private readonly Mock<IFileSystemHelper> _mockFileSystemHelper;
    private readonly Mock<IProcessRunner> _mockProcessRunner;
    private readonly HealthServiceManager _healthServiceManager;

    public HealthServiceManagerTests()
    {
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockDatabaseConnector = new Mock<IDatabaseConnector>();
        _mockFileSystemHelper = new Mock<IFileSystemHelper>();
        _mockProcessRunner = new Mock<IProcessRunner>();

        _healthServiceManager = new HealthServiceManager(
            "localhost",
            "5672",
            "testuser",
            "testpwd",
            "localhost",
            "3306",
            "test_db",
            "dbuser",
            "dbpwd",
            _mockConnectionFactory.Object,
            _mockDatabaseConnector.Object,
            _mockFileSystemHelper.Object,
            _mockProcessRunner.Object
        );
    }

    #region QueueExists Tests

    [Fact]
    public async Task QueueExists_WithExistingQueue_ReturnsTrue()
    {
        // Arrange
        string queueName = "logger_service";
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(queueName))
            .ReturnsAsync(true);

        // Act
        bool result = await _healthServiceManager.QueueExists(queueName);

        // Assert
        Assert.True(result);
        _mockConnectionFactory.Verify(x => x.CheckQueueExistsAsync(queueName), Times.Once);
    }

    [Fact]
    public async Task QueueExists_WithNonExistingQueue_ReturnsFalse()
    {
        // Arrange
        string queueName = "nonexistent_service";
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(queueName))
            .ReturnsAsync(false);

        // Act
        bool result = await _healthServiceManager.QueueExists(queueName);

        // Assert
        Assert.False(result);
        _mockConnectionFactory.Verify(x => x.CheckQueueExistsAsync(queueName), Times.Once);
    }

    [Fact]
    public async Task QueueExists_WithMultipleQueues_ChecksEach()
    {
        // Arrange
        var queues = new[] { "logger_service", "execution_service", "file_storage_service" };
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        foreach (var queue in queues)
        {
            await _healthServiceManager.QueueExists(queue);
        }

        // Assert
        _mockConnectionFactory.Verify(x => x.CheckQueueExistsAsync(It.IsAny<string>()), Times.Exactly(3));
    }

    #endregion

    #region UpdateRows Tests

    [Fact]
    public void UpdateRows_WithValidStatement_ExecutesUpdate()
    {
        // Arrange
        string updateStatement = "UPDATE cast_state_tracker SET state = 'OFFLINE' WHERE name = 'logger_service'";

        // Act
        _healthServiceManager.UpdateRows(updateStatement);

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(updateStatement), Times.Once);
    }

    [Fact]
    public void UpdateRows_WithMultipleStatements_ExecutesAll()
    {
        // Arrange
        var statements = new[]
        {
            "UPDATE cast_state_tracker SET state = 'OFFLINE' WHERE name = 'logger_service'",
            "UPDATE cast_state_tracker SET state = 'OFFLINE' WHERE name = 'execution_service'",
            "UPDATE cast_state_tracker SET state = 'OFFLINE' WHERE name = 'file_storage_service'"
        };

        // Act
        foreach (var statement in statements)
        {
            _healthServiceManager.UpdateRows(statement);
        }

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.IsAny<string>()), Times.Exactly(3));
    }

    #endregion

    #region GetServiceState Tests

    [Fact]
    public void GetServiceState_WithOnlineService_ReturnsOnline()
    {
        // Arrange
        string serviceName = "logger_service";
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns("ONLINE");

        // Act
        string result = _healthServiceManager.GetServiceState(serviceName);

        // Assert
        Assert.Equal("ONLINE", result);
        _mockDatabaseConnector.Verify(x => x.GetServiceState(serviceName), Times.Once);
    }

    [Fact]
    public void GetServiceState_WithOfflineService_ReturnsOffline()
    {
        // Arrange
        string serviceName = "execution_service";
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns("OFFLINE");

        // Act
        string result = _healthServiceManager.GetServiceState(serviceName);

        // Assert
        Assert.Equal("OFFLINE", result);
    }

    [Fact]
    public void GetServiceState_WithUnderConstructionService_ReturnsUnderConstruction()
    {
        // Arrange
        string serviceName = "new_service";
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns("UNDER CONSTRUCTION");

        // Act
        string result = _healthServiceManager.GetServiceState(serviceName);

        // Assert
        Assert.Equal("UNDER CONSTRUCTION", result);
    }

    #endregion

    #region UpdateServiceOffline Tests

    [Fact]
    public void UpdateServiceOffline_WithValidService_UpdatesDatabase()
    {
        // Arrange
        string serviceName = "logger_service";

        // Act
        _healthServiceManager.UpdateServiceOffline(serviceName);

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.Is<string>(
            sql => sql.Contains("UPDATE cast_state_tracker") &&
                   sql.Contains("OFFLINE") &&
                   sql.Contains(serviceName))), Times.Once);
    }

    #endregion

    #region GetClientServiceUUIDs Tests

    [Fact]
    public void GetClientServiceUUIDs_WithExistingServices_ReturnsListOfUUIDs()
    {
        // Arrange
        var expectedUUIDs = new List<string> { "uuid-1", "uuid-2", "uuid-3" };
        _mockDatabaseConnector.Setup(x => x.GetClientServiceUUIDs())
            .Returns(expectedUUIDs);

        // Act
        var result = _healthServiceManager.GetClientServiceUUIDs();

        // Assert
        Assert.Equal(expectedUUIDs.Count, result.Count);
        Assert.All(result, uuid => Assert.Contains(uuid, expectedUUIDs));
    }

    [Fact]
    public void GetClientServiceUUIDs_WithNoServices_ReturnsEmptyList()
    {
        // Arrange
        _mockDatabaseConnector.Setup(x => x.GetClientServiceUUIDs())
            .Returns(new List<string>());

        // Act
        var result = _healthServiceManager.GetClientServiceUUIDs();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetServiceStateInfo Tests

    [Fact]
    public void GetServiceStateInfo_WithValidUUID_ReturnsStateAndTime()
    {
        // Arrange
        string uuid = "test-uuid-123";
        var expectedTime = DateTime.Now.AddMinutes(-15);
        _mockDatabaseConnector.Setup(x => x.GetServiceStateInfo(uuid))
            .Returns(("RUNNING", expectedTime));

        // Act
        var (state, eventTime) = _healthServiceManager.GetServiceStateInfo(uuid);

        // Assert
        Assert.Equal("RUNNING", state);
        Assert.Equal(expectedTime, eventTime);
    }

    #endregion

    #region MarkServiceOffline Tests

    [Fact]
    public void MarkServiceOffline_WithValidUUID_InsertsOfflineState()
    {
        // Arrange
        string uuid = "test-uuid-456";

        // Act
        _healthServiceManager.MarkServiceOffline(uuid);

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.Is<string>(
            sql => sql.Contains("INSERT INTO state") &&
                   sql.Contains("OFFLINE") &&
                   sql.Contains(uuid))), Times.Once);
    }

    #endregion

    #region FindRabbitMQControlDirectory Tests

    [Fact]
    public void FindRabbitMQControlDirectory_WithExistingDirectory_ReturnsPath()
    {
        // Arrange
        string expectedPath = @"C:\Program Files\Rabbitmq Server\bin";
        _mockFileSystemHelper.Setup(x => x.FindFile(It.IsAny<string>(), "rabbitmqctl.bat"))
            .Returns(expectedPath);

        // Act
        string result = _healthServiceManager.FindRabbitMQControlDirectory();

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void FindRabbitMQControlDirectory_WithMissingDirectory_ReturnsEmpty()
    {
        // Arrange
        _mockFileSystemHelper.Setup(x => x.FindFile(It.IsAny<string>(), "rabbitmqctl.bat"))
            .Returns(string.Empty);

        // Act
        string result = _healthServiceManager.FindRabbitMQControlDirectory();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region DeleteEmptyQueue Tests

    [Fact]
    public void DeleteEmptyQueue_WithValidQueueAndDirectory_RunsRabbitMQCommand()
    {
        // Arrange
        string queueName = "client_service_uuid-123";
        string rabbitmqDirectory = @"C:\Program Files\Rabbitmq Server\bin";

        // Act
        _healthServiceManager.DeleteEmptyQueue(queueName, rabbitmqDirectory);

        // Assert
        _mockProcessRunner.Verify(x => x.Run(
            It.Is<string>(f => f.Contains("rabbitmqctl.bat")),
            It.Is<string>(a => a.Contains(queueName))), Times.Once);
    }

    #endregion

    #region IsServiceStateStale Tests

    [Fact]
    public void IsServiceStateStale_WithCompletedState_OlderThan30Minutes_ReturnsTrue()
    {
        // Arrange
        string state = "COMPLETED SUCCESSFULLY";
        var eventTime = DateTime.Now.AddMinutes(-31);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsServiceStateStale_WithCompletedState_LessThan30Minutes_ReturnsFalse()
    {
        // Arrange
        string state = "COMPLETED SUCCESSFULLY";
        var eventTime = DateTime.Now.AddMinutes(-15);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsServiceStateStale_WithAnyState_OlderThan720Minutes_ReturnsTrue()
    {
        // Arrange
        string state = "RUNNING";
        var eventTime = DateTime.Now.AddMinutes(-721);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsServiceStateStale_WithAnyState_LessThan720Minutes_ReturnsFalse()
    {
        // Arrange
        string state = "RUNNING";
        var eventTime = DateTime.Now.AddMinutes(-400);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsServiceStateStale_WithRunningState_RecentTime_ReturnsFalse()
    {
        // Arrange
        string state = "RUNNING";
        var eventTime = DateTime.Now.AddMinutes(-5);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("COMPLETED ", 31, true)]
    [InlineData("COMPLETED SUCCESSFULLY", 35, true)]
    [InlineData("COMPLETED ERROR", 25, false)]
    [InlineData("RUNNING", 100, false)]
    [InlineData("FAILED", 721, true)]
    public void IsServiceStateStale_WithVariousStates_ReturnsExpectedResult(string state, int minutesAgo, bool expected)
    {
        // Arrange
        var eventTime = DateTime.Now.AddMinutes(-minutesAgo);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ServiceHealthCheck_WhenQueueUnavailable_UpdatesServiceOffline()
    {
        // Arrange
        string serviceName = "logger_service";
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(serviceName))
            .ReturnsAsync(false);
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns("ONLINE");

        // Act
        bool queueExists = await _healthServiceManager.QueueExists(serviceName);
        if (!queueExists)
        {
            string currentState = _healthServiceManager.GetServiceState(serviceName);
            if (!currentState.Equals("OFFLINE") && !currentState.Equals("UNDER CONSTRUCTION"))
            {
                _healthServiceManager.UpdateServiceOffline(serviceName);
            }
        }

        // Assert
        Assert.False(queueExists);
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ClientServiceStateCheck_WhenStateStale_MarksOfflineAndDeletesQueue()
    {
        // Arrange
        string uuid = "test-uuid";
        var staleTime = DateTime.Now.AddMinutes(-31);
        _mockDatabaseConnector.Setup(x => x.GetServiceStateInfo(uuid))
            .Returns(("COMPLETED SUCCESSFULLY", staleTime));
        _mockFileSystemHelper.Setup(x => x.FindFile(It.IsAny<string>(), "rabbitmqctl.bat"))
            .Returns(@"C:\Program Files\Rabbitmq Server\bin");

        // Act
        var (state, eventTime) = _healthServiceManager.GetServiceStateInfo(uuid);
        if (_healthServiceManager.IsServiceStateStale(state, eventTime))
        {
            _healthServiceManager.MarkServiceOffline(uuid);
            _healthServiceManager.DeleteEmptyQueue($"client_service_{uuid}", @"C:\Program Files\Rabbitmq Server\bin");
        }

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.IsAny<string>()), Times.Once);
        _mockProcessRunner.Verify(x => x.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion
}

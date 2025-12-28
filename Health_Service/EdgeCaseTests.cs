using Xunit;
using Moq;
using System;
using System.Collections.Generic;

/// <summary>
/// Unit tests for edge cases and boundary conditions
/// </summary>
public class EdgeCaseTests
{
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IDatabaseConnector> _mockDatabaseConnector;
    private readonly Mock<IFileSystemHelper> _mockFileSystemHelper;
    private readonly Mock<IProcessRunner> _mockProcessRunner;
    private readonly HealthServiceManager _healthServiceManager;

    public EdgeCaseTests()
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

    #region Empty/Null Input Tests

    [Fact]
    public async Task QueueExists_WithEmptyQueueName_CallsConnectionFactory()
    {
        // Arrange
        string queueName = "";
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(queueName))
            .ReturnsAsync(false);

        // Act
        var result = await _healthServiceManager.QueueExists(queueName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetServiceState_WithEmptyServiceName_ReturnsEmpty()
    {
        // Arrange
        string serviceName = "";
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns(string.Empty);

        // Act
        var result = _healthServiceManager.GetServiceState(serviceName);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Time Boundary Tests

    [Fact]
    public void IsServiceStateStale_WithExactly30Minutes_ReturnsFalse()
    {
        // Arrange
        string state = "COMPLETED SUCCESSFULLY";
        var eventTime = DateTime.Now.AddMinutes(-30).AddMilliseconds(100);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsServiceStateStale_With30MinutesAnd1Second_ReturnsTrue()
    {
        // Arrange
        string state = "COMPLETED SUCCESSFULLY";
        var eventTime = DateTime.Now.AddMinutes(-30).AddSeconds(-1);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsServiceStateStale_WithExactly720Minutes_ReturnsFalse()
    {
        // Arrange
        string state = "RUNNING";
        var eventTime = DateTime.Now.AddMinutes(-720).AddMilliseconds(100);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsServiceStateStale_With720MinutesAnd1Second_ReturnsTrue()
    {
        // Arrange
        string state = "RUNNING";
        var eventTime = DateTime.Now.AddMinutes(-720).AddSeconds(-1);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Special Characters in Input Tests

    [Fact]
    public void UpdateServiceOffline_WithSpecialCharactersInServiceName_HandlesCorrectly()
    {
        // Arrange
        string serviceName = "service'with\"quotes";

        // Act
        _healthServiceManager.UpdateServiceOffline(serviceName);

        // Assert
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetServiceStateInfo_WithSpecialCharactersInUUID_HandlesCorrectly()
    {
        // Arrange
        string uuid = "uuid-with-special-chars-!@#";
        _mockDatabaseConnector.Setup(x => x.GetServiceStateInfo(uuid))
            .Returns(("RUNNING", DateTime.Now));

        // Act
        var (state, eventTime) = _healthServiceManager.GetServiceStateInfo(uuid);

        // Assert
        Assert.Equal("RUNNING", state);
    }

    #endregion

    #region Large Data Set Tests

    [Fact]
    public void GetClientServiceUUIDs_WithLargeNumberOfUUIDs_ReturnsAll()
    {
        // Arrange
        var largeUUIDList = new List<string>();
        for (int i = 0; i < 1000; i++)
        {
            largeUUIDList.Add($"uuid-{i:D4}");
        }
        _mockDatabaseConnector.Setup(x => x.GetClientServiceUUIDs())
            .Returns(largeUUIDList);

        // Act
        var result = _healthServiceManager.GetClientServiceUUIDs();

        // Assert
        Assert.Equal(1000, result.Count);
        _mockDatabaseConnector.Verify(x => x.GetClientServiceUUIDs(), Times.Once);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void IsServiceStateStale_WithMixedCaseCompletedState_ReturnsTrue()
    {
        // Arrange
        string state = "completed SUCCESSFULLY";
        var eventTime = DateTime.Now.AddMinutes(-31);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsServiceStateStale_WithUppercaseCompletedState_ReturnsTrue()
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
    public void IsServiceStateStale_WithLowercaseCompletedState_ReturnsTrue()
    {
        // Arrange
        string state = "completed successfully";
        var eventTime = DateTime.Now.AddMinutes(-31);

        // Act
        bool result = _healthServiceManager.IsServiceStateStale(state, eventTime);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public void MultipleQueueChecks_ConcurrentCalls_AllExecuteSuccessfully()
    {
        // Arrange
        _mockConnectionFactory.Setup(x => x.CheckQueueExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var tasks = new List<Task<bool>>();
        var queues = new[] { "queue1", "queue2", "queue3", "queue4", "queue5" };

        // Act
        foreach (var queue in queues)
        {
            tasks.Add(_healthServiceManager.QueueExists(queue));
        }
        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.All(tasks, t => Assert.True(t.Result));
        _mockConnectionFactory.Verify(x => x.CheckQueueExistsAsync(It.IsAny<string>()), Times.Exactly(5));
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void ServiceStateTransition_OnlineToOffline_UpdatesCorrectly()
    {
        // Arrange
        string serviceName = "logger_service";
        _mockDatabaseConnector.Setup(x => x.GetServiceState(serviceName))
            .Returns("ONLINE");

        // Act
        string initialState = _healthServiceManager.GetServiceState(serviceName);
        _healthServiceManager.UpdateServiceOffline(serviceName);

        // Assert
        Assert.Equal("ONLINE", initialState);
        _mockDatabaseConnector.Verify(x => x.ExecuteUpdate(It.IsAny<string>()), Times.Once);
    }

    #endregion
}

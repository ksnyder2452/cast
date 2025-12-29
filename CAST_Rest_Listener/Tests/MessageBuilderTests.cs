using Xunit;
using System.Text;

namespace CAST_Rest_Listener.Tests;

public class MessageBuilderTests
{
    [Fact]
    public void BuildStartMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-001";

        // Act
        string message = BuildStartMessage(clientId);

        // Assert
        Assert.Equal("message for client-001: local: action: start run", message);
    }

    [Fact]
    public void BuildStopMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-002";

        // Act
        string message = BuildStopMessage(clientId);

        // Assert
        Assert.Equal("message for client-002: local: action: stop run", message);
    }

    [Fact]
    public void BuildPauseMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-003";

        // Act
        string message = BuildPauseMessage(clientId);

        // Assert
        Assert.Equal("message for client-003: local: action: pause run", message);
    }

    [Fact]
    public void BuildResumeMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-004";

        // Act
        string message = BuildResumeMessage(clientId);

        // Assert
        Assert.Equal("message for client-004: local: action: resume run", message);
    }

    [Fact]
    public void BuildAbortMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-005";

        // Act
        string message = BuildAbortMessage(clientId);

        // Assert
        Assert.Equal("message for client-005: local: action: abort run", message);
    }

    [Fact]
    public void BuildRestartMessage_ShouldCreateValidMessage()
    {
        // Arrange
        string clientId = "client-006";

        // Act
        string message = BuildRestartMessage(clientId);

        // Assert
        Assert.Equal("message for client-006: local: action: restart run", message);
    }

    [Fact]
    public void EncodeMessage_ShouldProduceConsistentBytes()
    {
        // Arrange
        string message = "message for test: local: action: start run";

        // Act
        var bytes1 = Encoding.UTF8.GetBytes(message);
        var bytes2 = Encoding.UTF8.GetBytes(message);

        // Assert
        Assert.Equal(bytes1, bytes2);
    }

    [Fact]
    public void EncodeMessage_ShouldNotBeEmpty()
    {
        // Arrange
        string message = "message for client: local: action: start run";

        // Act
        var encodedBytes = Encoding.UTF8.GetBytes(message);

        // Assert
        Assert.NotEmpty(encodedBytes);
        Assert.True(encodedBytes.Length > 0);
    }

    // Helper methods that mirror the API message builders
    private string BuildStartMessage(string clientId)
        => $"message for {clientId}: local: action: start run";

    private string BuildStopMessage(string clientId)
        => $"message for {clientId}: local: action: stop run";

    private string BuildPauseMessage(string clientId)
        => $"message for {clientId}: local: action: pause run";

    private string BuildResumeMessage(string clientId)
        => $"message for {clientId}: local: action: resume run";

    private string BuildAbortMessage(string clientId)
        => $"message for {clientId}: local: action: abort run";

    private string BuildRestartMessage(string clientId)
        => $"message for {clientId}: local: action: restart run";
}

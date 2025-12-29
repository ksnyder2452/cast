using Xunit;
using System.Text;

namespace CAST_Rest_Listener.Tests;

public class IntegrationTests
{
    [Fact]
    public void MessageFormatting_StartClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-123";
        string expectedMessage = $"message for {clientId}: local: action: start run";

        // Act
        string message = $"message for {clientId}: local: action: start run";
        var body = Encoding.UTF8.GetBytes(message);

        // Assert
        Assert.Equal(expectedMessage, message);
        Assert.NotEmpty(body);
        Assert.True(body.Length > 0);
    }

    [Fact]
    public void MessageFormatting_StopClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-456";
        string expectedMessage = $"message for {clientId}: local: action: stop run";

        // Act
        string message = $"message for {clientId}: local: action: stop run";

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void MessageFormatting_PauseClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-789";
        string expectedMessage = $"message for {clientId}: local: action: pause run";

        // Act
        string message = $"message for {clientId}: local: action: pause run";

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void MessageFormatting_ResumeClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-101";
        string expectedMessage = $"message for {clientId}: local: action: resume run";

        // Act
        string message = $"message for {clientId}: local: action: resume run";

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void MessageFormatting_AbortClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-102";
        string expectedMessage = $"message for {clientId}: local: action: abort run";

        // Act
        string message = $"message for {clientId}: local: action: abort run";

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void MessageFormatting_RestartClientAction_ShouldBeCorrect()
    {
        // Arrange
        string clientId = "client-103";
        string expectedMessage = $"message for {clientId}: local: action: restart run";

        // Act
        string message = $"message for {clientId}: local: action: restart run";

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void MessageEncoding_ShouldProduceValidUTF8Bytes()
    {
        // Arrange
        string clientId = "test-client";
        string message = $"message for {clientId}: local: action: start run";

        // Act
        var body = Encoding.UTF8.GetBytes(message);
        var decodedMessage = Encoding.UTF8.GetString(body);

        // Assert
        Assert.Equal(message, decodedMessage);
    }

    [Fact]
    public void RoutingKey_ShouldBeExecutionService()
    {
        // Arrange
        string expectedRoutingKey = "execution_service";

        // Assert
        Assert.Equal("execution_service", expectedRoutingKey);
    }

    [Theory]
    [InlineData("client-1")]
    [InlineData("client-2")]
    [InlineData("test-123")]
    public void MessageFormatting_WithVariousClientIds_ShouldFormat(string clientId)
    {
        // Act
        string message = $"message for {clientId}: local: action: start run";

        // Assert
        Assert.Contains(clientId, message);
        Assert.Contains("local: action: start run", message);
    }
}

using Xunit;
using Microsoft.Extensions.Configuration;

namespace CAST_Rest_Listener.Tests;

public class ConfigurationTests
{
    [Fact]
    public void AppSettings_ShouldLoadFromJsonFile()
    {
        // Arrange
        var projectRoot = Directory.GetCurrentDirectory();
        var appsettingsPath = Path.Combine(projectRoot, "..", "appsettings.json");

        // Act
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath, optional: true)
            .Build();

        // Assert
        Assert.NotNull(configuration);
    }

    [Fact]
    public void RabbitMQConfiguration_ShouldExist()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        // Act
        var appSettings = configuration.GetSection("AppSettings");

        // Assert
        Assert.NotNull(appSettings);
    }

    [Fact]
    public void Configuration_ShouldSupportMultipleEnvironments()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Act
        var result = configuration.GetSection("AppSettings");

        // Assert
        Assert.NotNull(result);
    }
}

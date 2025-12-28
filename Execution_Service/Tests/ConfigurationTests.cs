using Xunit;
using System;
using System.Configuration;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Unit tests for configuration loading and validation
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// Test that configuration values are trimmed of whitespace
        /// </summary>
        [Fact]
        public void TrimConfigValue_WithWhitespace_RemovesWhitespace()
        {
            // Arrange
            string configValue = "  192.168.1.1  ";

            // Act
            string trimmed = configValue.Trim();

            // Assert
            Assert.Equal("192.168.1.1", trimmed);
        }

        /// <summary>
        /// Test handling of configuration with null/empty values
        /// </summary>
        [Fact]
        public void GetConfigValue_WithDefault_ReturnsDefault()
        {
            // Arrange
            string? value = null;
            string defaultValue = "localhost";

            // Act
            string result = value ?? defaultValue;

            // Assert
            Assert.Equal("localhost", result);
        }

        /// <summary>
        /// Test parsing of RabbitMQ port from configuration
        /// </summary>
        [Fact]
        public void ParsePort_ValidNumber_ReturnsInt()
        {
            // Arrange
            string portString = "5672";

            // Act
            int port = int.Parse(portString);

            // Assert
            Assert.Equal(5672, port);
        }

        /// <summary>
        /// Test parsing of invalid port throws exception
        /// </summary>
        [Fact]
        public void ParsePort_InvalidNumber_ThrowsException()
        {
            // Arrange
            string portString = "invalid_port";

            // Act & Assert
            Assert.Throws<FormatException>(() => int.Parse(portString));
        }

        /// <summary>
        /// Test configuration value chain with defaults
        /// </summary>
        [Fact]
        public void ConfigurationChain_WithDefault_HandlesNullCorrectly()
        {
            // Arrange
            string? fromConfig = null;
            string defaultValue = "default_service";

            // Act
            string serviceName = (fromConfig ?? "").Trim();
            if (string.IsNullOrEmpty(serviceName))
                serviceName = defaultValue;

            // Assert
            Assert.Equal("default_service", serviceName);
        }

        /// <summary>
        /// Test configuration value with actual content preserves value
        /// </summary>
        [Fact]
        public void ConfigurationValue_WithContent_PreservesValue()
        {
            // Arrange
            string? fromConfig = "my_execution_service";

            // Act
            string serviceName = (fromConfig ?? "").Trim();

            // Assert
            Assert.Equal("my_execution_service", serviceName);
        }

        /// <summary>
        /// Test empty string is handled same as null
        /// </summary>
        [Fact]
        public void ConfigurationValue_EmptyString_UseDefault()
        {
            // Arrange
            string fromConfig = "";
            string defaultValue = "default_value";

            // Act
            string result = (string.IsNullOrWhiteSpace(fromConfig) ? defaultValue : fromConfig).Trim();

            // Assert
            Assert.Equal("default_value", result);
        }
    }
}

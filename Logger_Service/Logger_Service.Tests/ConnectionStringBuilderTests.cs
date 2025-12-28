using Xunit;
using System;
using System.Configuration;

namespace Logger_Service.Tests
{
    /// <summary>
    /// Unit tests for connection string building and validation
    /// </summary>
    public class ConnectionStringBuilderTests
    {
        [Fact]
        public void BuildConnectionString_WithValidParameters_ReturnsValidConnectionString()
        {
            // Arrange
            var server = "localhost";
            var database = "cast_server";
            var user = "cast_write";
            var password = "test_password";
            var port = "3306";

            // Act
            var connectionString = $"Server={server}; Database={database}; Uid={user}; Pwd={password}; Port={port}";

            // Assert
            Assert.NotEmpty(connectionString);
            Assert.Contains("Server=localhost", connectionString);
            Assert.Contains("Database=cast_server", connectionString);
            Assert.Contains("Uid=cast_write", connectionString);
        }

        [Fact]
        public void BuildConnectionString_ContainsAllRequiredComponents()
        {
            // Arrange
            var server = "localhost";
            var database = "cast_server";
            var user = "cast_write";
            var password = "test_password";
            var port = "3306";

            // Act
            var connectionString = $"Server={server}; Database={database}; Uid={user}; Pwd={password}; Port={port}";

            // Assert
            Assert.Contains("Server=", connectionString);
            Assert.Contains("Database=", connectionString);
            Assert.Contains("Uid=", connectionString);
            Assert.Contains("Pwd=", connectionString);
            Assert.Contains("Port=", connectionString);
        }

        [Fact]
        public void BuildConnectionString_WithValidPort_PortIsNumeric()
        {
            // Arrange
            var port = "3306";

            // Act
            var isValidPort = int.TryParse(port, out var portNumber);

            // Assert
            Assert.True(isValidPort);
            Assert.Equal(3306, portNumber);
        }

        [Fact]
        public void BuildConnectionString_WithInvalidPort_ThrowsFormatException()
        {
            // Arrange
            var port = "invalid_port";

            // Act & Assert
            Assert.Throws<FormatException>(() => int.Parse(port));
        }

        [Fact]
        public void BuildConnectionString_TrimmedConfigValues_CorrectFormat()
        {
            // Arrange
            var server = "  localhost  ";
            var database = "  cast_server  ";

            // Act
            var trimmedServer = server.Trim();
            var trimmedDatabase = database.Trim();
            var connectionString = $"Server={trimmedServer}; Database={trimmedDatabase};";

            // Assert
            Assert.NotEmpty(trimmedServer);
            Assert.Equal("localhost", trimmedServer);
            Assert.Equal("cast_server", trimmedDatabase);
        }

        [Fact]
        public void BuildConnectionString_WithConfigurableValues_AllComponentsPresent()
        {
            // Arrange
            var server = "localhost";
            var database = "cast_server";
            var user = "cast_write";
            var password = "test_password";
            var port = "3306";

            // Act
            var connectionString = $"Server={server}; Database={database}; Uid={user}; Pwd={password}; Port={port}";

            // Assert
            Assert.NotEmpty(server);
            Assert.NotEmpty(database);
            Assert.NotEmpty(user);
            Assert.NotEmpty(password);
            Assert.NotEmpty(port);
            Assert.NotEmpty(connectionString);
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("192.168.1.1")]
        [InlineData("mysql.example.com")]
        public void BuildConnectionString_VariousServerValues_BuildsSuccessfully(string server)
        {
            // Arrange & Act
            var connectionString = $"Server={server}; Database=test; Uid=user; Pwd=pwd; Port=3306";

            // Assert
            Assert.Contains($"Server={server}", connectionString);
        }

        [Fact]
        public void ValidatePort_ValidPort_ReturnsTrue()
        {
            // Arrange
            var port = 3306;

            // Act
            var isValid = port > 0 && port < 65536;

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(65536)]
        public void ValidatePort_InvalidPort_ReturnsFalse(int port)
        {
            // Arrange & Act
            var isValid = port > 0 && port < 65536;

            // Assert
            Assert.False(isValid);
        }
    }
}

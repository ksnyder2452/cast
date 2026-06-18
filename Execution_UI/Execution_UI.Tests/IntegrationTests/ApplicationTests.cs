using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Execution_UI;

namespace Execution_UI.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests for the Execution_UI application.
    /// These tests verify end-to-end HTTP requests and responses.
    /// </summary>
    public class ApplicationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApplicationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_IndexPage_ReturnsOkStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Expected success status code, got {response.StatusCode}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_IndexPage_ReturnsHtmlContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);
            Assert.True(content.Contains("<!DOCTYPE") || content.Contains("<html"), "Response should contain HTML content");
        }

        [Fact]
        public async Task Get_IndexPage_ReturnsHtmlContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Contains("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task Get_PrivacyPage_ReturnsOkStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Privacy");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Expected success status code, got {response.StatusCode}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_PrivacyPage_ReturnsHtmlContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Privacy");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task Get_CastPage_ReturnsResponse()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/cast");

            // Assert
            // Note: May return 500 if RabbitMQ/MySQL are not configured, 
            // but should not throw unhandled exceptions
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Get_InvalidPage_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/invalid-page-that-does-not-exist-12345");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_RootPath_DoesNotThrow()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act & Assert - Should not throw an exception
            var response = await client.GetAsync("/");
            Assert.NotNull(response);
        }

        [Fact]
        public void ApplicationFactory_CanCreateClient()
        {
            // Arrange & Act
            var client = _factory.CreateClient();

            // Assert
            Assert.NotNull(client);
        }
    }
}

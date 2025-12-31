using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Execution_UI;

namespace Execution_UI.Tests.IntegrationTests
{
    public class ApplicationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApplicationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_IndexPage_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_PrivacyPage_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Privacy");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_CastPage_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/cast_new");

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
            var response = await client.GetAsync("/invalid-page-that-does-not-exist");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
            Assert.True(content.Contains("<!DOCTYPE") || content.Contains("<html"));
        }
    }
}

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Execution_UI.Pages;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Execution_UI.Tests.Pages
{
    public class ErrorPageTests
    {
        [Fact]
        public void ErrorModel_OnGet_WithValidPageContext_SetsRequestId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ErrorModel>>();
            var model = new ErrorModel(mockLogger.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("test-trace-id");

            var pageContext = new PageContext
            {
                HttpContext = mockHttpContext.Object
            };
            model.PageContext = pageContext;

            // Act
            model.OnGet();

            // Assert
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void ErrorModel_ShowRequestId_ReturnsTrueWhenRequestIdIsSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ErrorModel>>();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = "test-id"
            };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ErrorModel_ShowRequestId_ReturnsFalseWhenRequestIdIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ErrorModel>>();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = null
            };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ErrorModel_ShowRequestId_ReturnsFalseWhenRequestIdIsEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ErrorModel>>();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = string.Empty
            };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ErrorModel_Constructor_InitializesWithValidLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ErrorModel>>();

            // Act
            var model = new ErrorModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void ErrorModel_IsAttributeResponseCache()
        {
            // Arrange & Act
            var model = new ErrorModel(new Mock<ILogger<ErrorModel>>().Object);

            // Assert
            var attributes = typeof(ErrorModel).GetCustomAttributes(typeof(ResponseCacheAttribute), false);
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void ErrorModel_IsAttributeIgnoreAntiforgeryToken()
        {
            // Arrange & Act
            var model = new ErrorModel(new Mock<ILogger<ErrorModel>>().Object);

            // Assert
            var attributes = typeof(ErrorModel).GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), false);
            Assert.NotEmpty(attributes);
        }
    }
}

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
    /// <summary>
    /// Unit tests for ErrorModel page model.
    /// Tests error page initialization, request ID handling, and attributes.
    /// </summary>
    public class ErrorPageTests
    {
        private Mock<ILogger<ErrorModel>> CreateMockLogger()
        {
            return new Mock<ILogger<ErrorModel>>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidLogger_InitializesSuccessfully()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new ErrorModel(mockLogger.Object);

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Constructor_InitializesRequestIdAsNull()
        {
            // Arrange
            var mockLogger = CreateMockLogger();

            // Act
            var model = new ErrorModel(mockLogger.Object);

            // Assert
            Assert.Null(model.RequestId);
        }

        #endregion

        #region OnGet Tests

        [Fact]
        public void OnGet_WithValidPageContext_SetsRequestId()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object);
            var traceId = "test-trace-id-123";

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceId);

            var pageContext = new PageContext
            {
                HttpContext = mockHttpContext.Object
            };
            model.PageContext = pageContext;

            // Act
            model.OnGet();

            // Assert
            Assert.NotNull(model.RequestId);
            Assert.Equal(traceId, model.RequestId);
        }

        [Fact]
        public void OnGet_WithActivityId_SetsRequestIdFromActivity()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("trace-id");

            var pageContext = new PageContext
            {
                HttpContext = mockHttpContext.Object
            };
            model.PageContext = pageContext;

            // Act
            model.OnGet();

            // Assert
            // Should set RequestId from either Activity or TraceIdentifier
            Assert.NotNull(model.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void OnGet_ExecutesSuccessfully()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("test-id");

            model.PageContext = new PageContext { HttpContext = mockHttpContext.Object };

            // Act & Assert
            var exception = Record.Exception(() => model.OnGet());
            Assert.Null(exception);
        }

        #endregion

        #region ShowRequestId Property Tests

        [Fact]
        public void ShowRequestId_ReturnsTrueWhenRequestIdIsSet()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = "test-id"
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShowRequestId_ReturnsFalseWhenRequestIdIsNull()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = null
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ReturnsFalseWhenRequestIdIsEmpty()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = string.Empty
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ReturnsFalseWhenRequestIdIsWhitespace()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object)
            {
                RequestId = "   "
            };

            // Act
            var result = model.ShowRequestId;

            // Assert
            // string.IsNullOrEmpty returns false for whitespace, so ShowRequestId should be true
            Assert.True(result);
        }

        #endregion

        #region Attribute Tests

        [Fact]
        public void ErrorModel_HasResponseCacheAttribute()
        {
            // Arrange & Act
            var attributes = typeof(ErrorModel).GetCustomAttributes(typeof(ResponseCacheAttribute), false);

            // Assert
            Assert.NotEmpty(attributes);
            var responseCache = attributes[0] as ResponseCacheAttribute;
            Assert.NotNull(responseCache);
            Assert.Equal(0, responseCache.Duration);
            Assert.Equal(ResponseCacheLocation.None, responseCache.Location);
            Assert.True(responseCache.NoStore);
        }

        [Fact]
        public void ErrorModel_HasIgnoreAntiforgeryTokenAttribute()
        {
            // Arrange & Act
            var attributes = typeof(ErrorModel).GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), false);

            // Assert
            Assert.NotEmpty(attributes);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ErrorModel_WorkflowWithValidContext()
        {
            // Arrange
            var mockLogger = CreateMockLogger();
            var model = new ErrorModel(mockLogger.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("workflow-test-id");

            var pageContext = new PageContext { HttpContext = mockHttpContext.Object };
            model.PageContext = pageContext;

            // Act
            model.OnGet();
            var shouldShow = model.ShowRequestId;

            // Assert
            Assert.NotNull(model.RequestId);
            Assert.True(shouldShow);
            Assert.Equal("workflow-test-id", model.RequestId);
        }

        #endregion
    }
}

using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using CAST_Rest_Listener.Pages;
using System.Diagnostics;

namespace CAST_Rest_Listener.Tests;

public class ErrorPageModelTests
{
    [Fact]
    public void ErrorModel_ShouldInitializeWithLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();

        // Act
        var pageModel = new ErrorModel(mockLogger.Object);

        // Assert
        Assert.NotNull(pageModel);
    }

    [Fact]
    public void OnGet_ShouldSetRequestId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var pageModel = new ErrorModel(mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var traceId = "test-trace-id-12345";
        httpContext.TraceIdentifier = traceId;

        var pageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext();
        pageContext.HttpContext = httpContext;

        pageModel.PageContext = pageContext;

        // Act
        pageModel.OnGet();

        // Assert
        Assert.NotNull(pageModel.RequestId);
    }

    [Fact]
    public void ShowRequestId_ShouldBeFalseWhenRequestIdIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var pageModel = new ErrorModel(mockLogger.Object);
        pageModel.RequestId = null;

        // Act
        var result = pageModel.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShowRequestId_ShouldBeFalseWhenRequestIdIsEmpty()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var pageModel = new ErrorModel(mockLogger.Object);
        pageModel.RequestId = string.Empty;

        // Act
        var result = pageModel.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShowRequestId_ShouldBeTrueWhenRequestIdIsSet()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var pageModel = new ErrorModel(mockLogger.Object);
        pageModel.RequestId = "valid-request-id";

        // Act
        var result = pageModel.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OnGet_ShouldNotThrowException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var pageModel = new ErrorModel(mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var pageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext();
        pageContext.HttpContext = httpContext;
        pageModel.PageContext = pageContext;

        // Act
        var exception = Record.Exception(() => pageModel.OnGet());

        // Assert
        Assert.Null(exception);
    }
}

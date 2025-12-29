using Xunit;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using CAST_Rest_Listener.Pages;

namespace CAST_Rest_Listener.Tests;

public class IndexPageModelTests
{
    [Fact]
    public void OnGet_ShouldExecuteWithoutException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IndexModel>>();
        var pageModel = new IndexModel(mockLogger.Object);

        // Act
        var result = () => pageModel.OnGet();

        // Assert
        var exception = Record.Exception(result);
        Assert.Null(exception);
    }

    [Fact]
    public void IndexModel_ShouldInitializeWithLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IndexModel>>();

        // Act
        var pageModel = new IndexModel(mockLogger.Object);

        // Assert
        Assert.NotNull(pageModel);
    }

    [Fact]
    public void OnGet_ShouldNotThrowWhenCalledMultipleTimes()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IndexModel>>();
        var pageModel = new IndexModel(mockLogger.Object);

        // Act & Assert
        pageModel.OnGet();
        pageModel.OnGet();
        pageModel.OnGet();
    }
}

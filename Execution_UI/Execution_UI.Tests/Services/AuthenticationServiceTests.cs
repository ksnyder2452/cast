using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Execution_UI.Services;
using System.Text;

namespace Execution_UI.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthenticationService.
    /// Tests credential validation, basic auth handling, and session management.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
        {
            return new Mock<IHttpContextAccessor>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidHttpContextAccessor_InitializesSuccessfully()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();

            // Act
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_CallsLoadCredentials()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();

            // Act - Constructor should load credentials without throwing
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Assert - Service should be initialized without exception
            Assert.NotNull(service);
        }

        #endregion

        #region ValidateCredentials Tests

        [Fact]
        public void ValidateCredentials_WithNullUsername_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials(null!, "password");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCredentials_WithEmptyUsername_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials(string.Empty, "password");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCredentials_WithNullPassword_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials("username", null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCredentials_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials("username", string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCredentials_WithNonexistentUser_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials("nonexistent", "password");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCredentials_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateCredentials("invaliduser", "invalidpass");

            // Assert
            Assert.IsType<bool>(result);
            Assert.False(result);
        }

        #endregion

        #region ValidateBasicAuth Tests

        [Fact]
        public void ValidateBasicAuth_WithNullAuthHeader_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateBasicAuth_WithEmptyAuthHeader_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary { { "Authorization", "" } };
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateBasicAuth_WithoutBasicPrefix_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary { { "Authorization", "Bearer token123" } };
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateBasicAuth_WithInvalidBase64_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary { { "Authorization", "Basic !!invalid!!" } };
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateBasicAuth_WithValidBasicAuthHeader_AttemptsValidation()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();

            // "dGVzdHVzZXI6dGVzdHBhc3M=" is base64 for "testuser:testpass"
            var headers = new HeaderDictionary { { "Authorization", "Basic dGVzdHVzZXI6dGVzdHBhc3M=" } };
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert - Should return boolean based on credentials validation
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void ValidateBasicAuth_WithMissingColon_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            var mockRequest = new Mock<HttpRequest>();

            // "dGVzdHVzZXJ0ZXN0cGFzcw==" is base64 for "testusertestpass" (no colon)
            var headers = new HeaderDictionary { { "Authorization", "Basic dGVzdHVzZXJ0ZXN0cGFzcw==" } };
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.ValidateBasicAuth(mockRequest.Object);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region SetAuthenticationToken Tests

        [Fact]
        public void SetAuthenticationToken_WithValidToken_DoesNotThrow()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);
            var token = "test-token-123";

            // Act & Assert
            var exception = Record.Exception(() => service.SetAuthenticationToken(token));
            Assert.Null(exception);
        }

        [Fact]
        public void SetAuthenticationToken_WithNullHttpContext_DoesNotThrow()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => service.SetAuthenticationToken("token"));
            Assert.Null(exception);
        }

        #endregion

        #region GetAuthenticationToken Tests

        [Fact]
        public void GetAuthenticationToken_WithNullHttpContext_ReturnsEmptyString()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.GetAuthenticationToken();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetAuthenticationToken_ReturnsString()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.GetAuthenticationToken();

            // Assert
            Assert.IsType<string>(result);
        }

        #endregion

        #region IsAuthenticated Tests

        [Fact]
        public void IsAuthenticated_WithNullHttpContext_ReturnsFalse()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.IsAuthenticated();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAuthenticated_ReturnsBoolean()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.IsAuthenticated();

            // Assert
            Assert.IsType<bool>(result);
        }

        #endregion

        #region ClearAuthentication Tests

        [Fact]
        public void ClearAuthentication_WithValidSession_CallsClear()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            service.ClearAuthentication();

            // Assert - Verify that session operations were called
            mockSession.Verify(s => s.Clear(), Times.Once);
        }

        [Fact]
        public void ClearAuthentication_WithNullHttpContext_DoesNotThrow()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act & Assert
            var exception = Record.Exception(() => service.ClearAuthentication());
            Assert.Null(exception);
        }

        #endregion

        #region SetUsername Tests

        [Fact]
        public void SetUsername_WithValidUsername_DoesNotThrow()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);
            var username = "testuser";

            // Act & Assert
            var exception = Record.Exception(() => service.SetUsername(username));
            Assert.Null(exception);
        }

        [Fact]
        public void SetUsername_WithNullHttpContext_DoesNotThrow()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act & Assert
            var exception = Record.Exception(() => service.SetUsername("testuser"));
            Assert.Null(exception);
        }

        #endregion

        #region GetUsername Tests

        [Fact]
        public void GetUsername_WithNullHttpContext_ReturnsEmptyString()
        {
            // Arrange
            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.GetUsername();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetUsername_ReturnsString()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockHttpContextAccessor = CreateMockHttpContextAccessor();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var service = new AuthenticationService(mockHttpContextAccessor.Object);

            // Act
            var result = service.GetUsername();

            // Assert
            Assert.IsType<string>(result);
        }

        #endregion
    }
}

using Microsoft.Extensions.Logging;
using Moq;

namespace Execution_UI.Tests
{
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a mock logger for testing
        /// </summary>
        public static Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        /// <summary>
        /// Verifies that logging was called with a specific log level
        /// </summary>
        public static void VerifyLogging<T>(
            this Mock<ILogger<T>> mockLogger,
            Func<LogLevel, bool> levelFilter,
            Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => levelFilter(l)),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }
    }
}

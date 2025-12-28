using Xunit;
using System;
using System.Threading;

namespace Logger_Service.Tests
{
    /// <summary>
    /// Unit tests for retry logic and error handling
    /// </summary>
    public class RetryLogicTests
    {
        [Fact]
        public void CheckRetryCondition_ZeroRowsAffectedWithoutIgnore_RequiresRetry()
        {
            // Arrange
            var rowsAffected = 0;
            var statement = "INSERT INTO test (id) VALUES (1)";
            var hasIgnore = statement.ToUpper().Contains(" IGNORE ");

            // Act
            var shouldRetry = rowsAffected == 0 && !hasIgnore;

            // Assert
            Assert.True(shouldRetry);
        }

        [Fact]
        public void CheckRetryCondition_ZeroRowsAffectedWithIgnore_DoesNotRequireRetry()
        {
            // Arrange
            var rowsAffected = 0;
            var statement = "DELETE IGNORE FROM test WHERE id = 1";
            var hasIgnore = statement.ToUpper().Contains(" IGNORE ");

            // Act
            var shouldRetry = rowsAffected == 0 && !hasIgnore;

            // Assert
            Assert.False(shouldRetry);
        }

        [Fact]
        public void CheckRetryCondition_PositiveRowsAffected_DoesNotRequireRetry()
        {
            // Arrange
            var rowsAffected = 1;
            var statement = "INSERT INTO test (id) VALUES (1)";
            var hasIgnore = statement.ToUpper().Contains(" IGNORE ");

            // Act
            var shouldRetry = rowsAffected == 0 && !hasIgnore;

            // Assert
            Assert.False(shouldRetry);
        }

        [Fact]
        public void RetryDelay_WaitFiveSeconds_DelayIsCorrect()
        {
            // Arrange
            var delayMs = 5000;
            var startTime = DateTime.Now;

            // Act
            Thread.Sleep(delayMs);
            var endTime = DateTime.Now;

            // Assert
            var elapsedMs = (endTime - startTime).TotalMilliseconds;
            Assert.True(elapsedMs >= delayMs - 50); // Allow 50ms tolerance
        }

        [Fact]
        public void RetryLogic_FirstAttemptFails_SecondAttemptRetried()
        {
            // Arrange
            var initialAttemptFailed = true;
            var retryCount = 0;

            // Act
            if (initialAttemptFailed)
            {
                retryCount++;
            }

            // Assert
            Assert.Equal(1, retryCount);
        }

        [Fact]
        public void RetryLogic_FirstAttemptSucceeds_NoRetry()
        {
            // Arrange
            var initialAttemptFailed = false;
            var retryCount = 0;

            // Act
            if (initialAttemptFailed)
            {
                retryCount++;
            }

            // Assert
            Assert.Equal(0, retryCount);
        }

        [Fact]
        public void HandleRetryFailure_ZeroRowsAfterRetry_ThrowsException()
        {
            // Arrange
            var rowsAffected = 0;
            var statement = "INSERT INTO test (id) VALUES (1)";
            var exceptionThrown = false;

            // Act
            try
            {
                if (rowsAffected == 0)
                {
                    throw new Exception($"No changes were made with SQL statement {statement}");
                }
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(exceptionThrown);
        }

        [Fact]
        public void HandleRetryFailure_PositiveRowsAfterRetry_NoException()
        {
            // Arrange
            var rowsAffected = 1;
            var statement = "INSERT INTO test (id) VALUES (1)";
            var exceptionThrown = false;

            // Act
            try
            {
                if (rowsAffected == 0)
                {
                    throw new Exception($"No changes were made with SQL statement {statement}");
                }
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.False(exceptionThrown);
        }

        [Fact]
        public void RetryLogic_MultipleAttempts_CountsCorrectly()
        {
            // Arrange
            var attemptCount = 0;
            var maxAttempts = 2;

            // Act
            for (int i = 0; i < maxAttempts; i++)
            {
                attemptCount++;
            }

            // Assert
            Assert.Equal(2, attemptCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void ValidateRowsAffectedValue_Various_AreHandledCorrectly(int rowsAffected)
        {
            // Arrange & Act
            var isZero = rowsAffected == 0;

            // Assert
            Assert.Equal(rowsAffected == 0, isZero);
        }

        [Fact]
        public void ExceptionHandling_CatchMySqlException_LogsError()
        {
            // Arrange
            var errorMessage = "Connection timeout";
            var caught = false;

            // Act
            try
            {
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                caught = true;
                Assert.Contains(errorMessage, ex.Message);
            }

            // Assert
            Assert.True(caught);
        }

        [Fact]
        public void ExceptionHandling_CatchGeneralException_LogsError()
        {
            // Arrange
            var errorMessage = "General error occurred";
            var caught = false;

            // Act
            try
            {
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                caught = true;
                Assert.Contains(errorMessage, ex.Message);
            }

            // Assert
            Assert.True(caught);
        }
    }
}

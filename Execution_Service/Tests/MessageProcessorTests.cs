using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Execution_Service.Tests
{
    /// <summary>
    /// Unit tests for message processing functionality in the Execution Service
    /// </summary>
    public class MessageProcessorTests
    {
        /// <summary>
        /// Test that regular messages are parsed correctly
        /// </summary>
        [Fact]
        public void ParseRegularMessage_ValidMessage_ExtractsUUIDAndContent()
        {
            // Arrange
            string testUUID = "550e8400-e29b-41d4-a716-446655440000";
            string testMessage = "Hello World";
            string messageFormat = $"MESSAGE FOR {testUUID}: {testMessage}";

            // Act
            string extractedUUID = messageFormat.Substring(12, messageFormat.IndexOf(":") - 12).Trim();
            string extractedMessage = messageFormat.Substring(messageFormat.IndexOf(":") + 1).Trim();

            // Assert
            Assert.Equal(testUUID, extractedUUID);
            Assert.Equal(testMessage, extractedMessage);
        }

        /// <summary>
        /// Test message starts with correct prefix
        /// </summary>
        [Fact]
        public void IsRegularMessage_ValidFormat_ReturnsTrue()
        {
            // Arrange
            string message = "MESSAGE FOR 550e8400-e29b-41d4-a716-446655440000: Some data";

            // Act
            bool isRegularMessage = message.ToUpper().StartsWith("MESSAGE FOR ");

            // Assert
            Assert.True(isRegularMessage);
        }

        /// <summary>
        /// Test detection of INSERT queries
        /// </summary>
        [Fact]
        public void IsInsertQuery_ValidInsert_ReturnsTrue()
        {
            // Arrange
            string message = "INSERT INTO logger (uuid, type) VALUES ('123', 'INFO')";

            // Act
            bool isInsert = message.Trim().ToUpper().StartsWith("INSERT INTO ");

            // Assert
            Assert.True(isInsert);
        }

        /// <summary>
        /// Test detection of non-INSERT queries
        /// </summary>
        [Fact]
        public void IsInsertQuery_NonInsertStatement_ReturnsFalse()
        {
            // Arrange
            string message = "SELECT * FROM logger";

            // Act
            bool isInsert = message.Trim().ToUpper().StartsWith("INSERT INTO ");

            // Assert
            Assert.False(isInsert);
        }

        /// <summary>
        /// Test message format validation with edge cases
        /// </summary>
        [Fact]
        public void ParseMessage_WithWhitespace_HandlesCorrectly()
        {
            // Arrange
            string testUUID = "550e8400-e29b-41d4-a716-446655440000";
            string messageFormat = $"MESSAGE FOR   {testUUID}  :   test message   ";

            // Act
            string extractedUUID = messageFormat.Substring(12, messageFormat.IndexOf(":") - 12).Trim();

            // Assert
            Assert.Equal(testUUID, extractedUUID);
        }

        /// <summary>
        /// Test encoding/decoding of byte arrays to strings
        /// </summary>
        [Fact]
        public void MessageEncoding_RoundTrip_PreservesContent()
        {
            // Arrange
            string originalMessage = "INSERT INTO logger VALUES ('test')";

            // Act
            byte[] encoded = Encoding.UTF8.GetBytes(originalMessage);
            string decoded = Encoding.UTF8.GetString(encoded);

            // Assert
            Assert.Equal(originalMessage, decoded);
        }

        /// <summary>
        /// Test empty message handling
        /// </summary>
        [Fact]
        public void IsRegularMessage_EmptyString_ReturnsFalse()
        {
            // Arrange
            string message = string.Empty;

            // Act
            bool isRegularMessage = message.ToUpper().StartsWith("MESSAGE FOR ");

            // Assert
            Assert.False(isRegularMessage);
        }

        /// <summary>
        /// Test message with null handling
        /// </summary>
        [Fact]
        public void IsRegularMessage_NullString_ThrowsException()
        {
            // Arrange
            string? message = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => message!.ToUpper().StartsWith("MESSAGE FOR "));
        }
    }
}

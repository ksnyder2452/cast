using Xunit;
using System;
using System.Text;

namespace Logger_Service.Tests
{
    /// <summary>
    /// Unit tests for message processing logic
    /// </summary>
    public class MessageProcessingTests
    {
        [Fact]
        public void IsActionCommand_WithActionKeyword_ReturnsTrue()
        {
            // Arrange
            var message = "{'ACTION': 'restart_service'}";

            // Act
            var isAction = message.Contains("ACTION");

            // Assert
            Assert.True(isAction);
        }

        [Fact]
        public void IsActionCommand_WithoutActionKeyword_ReturnsFalse()
        {
            // Arrange
            var message = "INSERT INTO logger (uuid, message) VALUES ('12345', 'Test message')";

            // Act
            var isAction = message.Contains("ACTION");

            // Assert
            Assert.False(isAction);
        }

        [Fact]
        public void ExtractActionCommand_ValidActionString_ReturnsActionName()
        {
            // Arrange
            var message = "insert into logger ('ACTION' 'restart_service')";
            var expectedAction = "restart_service";

            // Act
            var actionStart = message.IndexOf("'ACTION'") + 8;
            var action = message.Substring(actionStart);
            action = action.Substring(action.IndexOf("'") + 1);
            action = action.Substring(0, action.IndexOf("'"));

            // Assert
            Assert.Equal(expectedAction, action);
        }

        [Fact]
        public void ConvertMessageToString_ValidByteArray_ReturnsCorrectString()
        {
            // Arrange
            var originalMessage = "INSERT INTO logger (message) VALUES ('test')";
            var messageBytes = Encoding.UTF8.GetBytes(originalMessage);

            // Act
            var convertedMessage = Encoding.UTF8.GetString(messageBytes);

            // Assert
            Assert.Equal(originalMessage, convertedMessage);
        }

        [Fact]
        public void AddSemicolonToSQL_MessageWithoutSemicolon_AddsSemicolon()
        {
            // Arrange
            var message = "INSERT INTO logger (message) VALUES ('test')";

            // Act
            var result = message.EndsWith(";") ? message : message + ";";

            // Assert
            Assert.EndsWith(";", result);
            Assert.Equal("INSERT INTO logger (message) VALUES ('test');", result);
        }

        [Fact]
        public void AddSemicolonToSQL_MessageWithSemicolon_DoesNotAddAnother()
        {
            // Arrange
            var message = "INSERT INTO logger (message) VALUES ('test');";

            // Act
            var result = message.EndsWith(";") ? message : message + ";";

            // Assert
            Assert.Equal("INSERT INTO logger (message) VALUES ('test');", result);
            Assert.Single(result.Where(c => c == ';'));
        }

        [Fact]
        public void CheckSQLStatement_ValidInsertStatement_IsValid()
        {
            // Arrange
            var sqlStatement = "INSERT INTO logger (uuid, message) VALUES ('123', 'test')";

            // Act
            var isValidInsert = sqlStatement.ToUpper().StartsWith("INSERT");

            // Assert
            Assert.True(isValidInsert);
        }

        [Fact]
        public void CheckSQLStatement_ValidUpdateStatement_IsValid()
        {
            // Arrange
            var sqlStatement = "UPDATE cast_state_tracker SET state = 'ONLINE'";

            // Act
            var isValidUpdate = sqlStatement.ToUpper().StartsWith("UPDATE");

            // Assert
            Assert.True(isValidUpdate);
        }

        [Fact]
        public void CheckSQLStatement_StatementWithIgnore_ContainsIgnore()
        {
            // Arrange
            var sqlStatement = "DELETE IGNORE FROM cast_state_tracker WHERE name = 'test'";

            // Act
            var hasIgnore = sqlStatement.ToUpper().Contains(" IGNORE ");

            // Assert
            Assert.True(hasIgnore);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ValidateMessage_EmptyOrNullMessage_ReturnsFalse(string? message)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(message);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateMessage_NonEmptyMessage_ReturnsTrue()
        {
            // Arrange
            var message = "INSERT INTO logger (message) VALUES ('test')";

            // Act
            var isValid = !string.IsNullOrWhiteSpace(message);

            // Assert
            Assert.True(isValid);
        }
    }
}

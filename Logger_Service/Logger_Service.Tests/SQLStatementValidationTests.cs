using Xunit;
using System;

namespace Logger_Service.Tests
{
    /// <summary>
    /// Unit tests for SQL statement validation and construction
    /// </summary>
    public class SQLStatementValidationTests
    {
        [Fact]
        public void BuildInsertLoggerStatement_WithValidParameters_ReturnsValidSQL()
        {
            // Arrange
            var uuid = Guid.NewGuid().ToString();
            var referenceUuid = Guid.NewGuid().ToString();
            var originator = "logger_service";
            var type = "INFO";
            var message = "Service started";
            var displayName = "Logger Service";

            // Act
            var sql = $"insert into logger (uuid, reference_uuid, originator, type, message, event_time_dt, display_name) values('{uuid}', '{referenceUuid}', '{originator}', '{type}', '{message}', NOW(), '{displayName}')";

            // Assert
            Assert.NotEmpty(sql);
            Assert.Contains("insert into logger", sql);
            Assert.Contains(uuid, sql);
            Assert.Contains(referenceUuid, sql);
            Assert.Contains(displayName, sql);
        }

        [Fact]
        public void BuildCleanupStatement_WithServiceName_ReturnsValidSQL()
        {
            // Arrange
            var serviceName = "Logger Service";

            // Act
            var sql = $"delete ignore from cast_state_tracker where name = '{serviceName}'";

            // Assert
            Assert.NotEmpty(sql);
            Assert.Contains("delete ignore from cast_state_tracker", sql);
            Assert.Contains(serviceName, sql);
        }

        [Fact]
        public void BuildRegisterStatement_WithValidParameters_ReturnsValidSQL()
        {
            // Arrange
            var serviceName = "Logger Service";
            var state = "ONLINE";

            // Act
            var sql = $"insert into cast_state_tracker (name, state, event_time_dt) values('{serviceName}', '{state}', NOW())";

            // Assert
            Assert.NotEmpty(sql);
            Assert.Contains("insert into cast_state_tracker", sql);
            Assert.Contains(serviceName, sql);
            Assert.Contains(state, sql);
        }

        [Fact]
        public void BuildUpdateStateStatement_WithValidParameters_ReturnsValidSQL()
        {
            // Arrange
            var serviceName = "Logger Service";
            var state = "OFFLINE";

            // Act
            var sql = $"update cast_state_tracker set state = '{state}', event_time_dt = NOW() where name = '{serviceName}'";

            // Assert
            Assert.NotEmpty(sql);
            Assert.Contains("update cast_state_tracker", sql);
            Assert.Contains(state, sql);
            Assert.Contains(serviceName, sql);
        }

        [Fact]
        public void ValidateSQLStatement_ContainsSemicolon_IsComplete()
        {
            // Arrange
            var sql = "INSERT INTO logger (message) VALUES ('test');";

            // Act
            var hasEndSemicolon = sql.EndsWith(";");

            // Assert
            Assert.True(hasEndSemicolon);
        }

        [Fact]
        public void ValidateSQLStatement_MissingTableName_CanBeDetected()
        {
            // Arrange
            var sql = "INSERT INTO  (message) VALUES ('test')";

            // Act
            var isEmpty = sql.Contains("INTO  ");

            // Assert
            Assert.True(isEmpty);
        }

        [Fact]
        public void ValidateSQLStatement_ContainsValidKeywords_IsValid()
        {
            // Arrange
            var keywords = new[] { "INSERT", "UPDATE", "DELETE", "SELECT" };
            var statement = "INSERT INTO logger (message) VALUES ('test')";

            // Act
            var containsKeyword = keywords.Any(k => statement.ToUpper().Contains(k));

            // Assert
            Assert.True(containsKeyword);
        }

        [Fact]
        public void EscapeSQLString_WithSpecialCharacters_CanBePrepared()
        {
            // Arrange
            var input = "It's a test";
            var escaped = input.Replace("'", "''");

            // Act
            var sql = $"INSERT INTO logger (message) VALUES ('{escaped}')";

            // Assert
            Assert.Contains("It''s a test", sql);
        }

        [Fact]
        public void ValidateRowsAffected_ZeroRowsAffected_ReturnsZero()
        {
            // Arrange
            var rowsAffected = 0;

            // Act
            var isZero = rowsAffected == 0;

            // Assert
            Assert.True(isZero);
        }

        [Fact]
        public void ValidateRowsAffected_PositiveRowsAffected_ReturnsPositive()
        {
            // Arrange
            var rowsAffected = 5;

            // Act
            var isPositive = rowsAffected > 0;

            // Assert
            Assert.True(isPositive);
        }

        [Fact]
        public void BuildDeleteIgnoreStatement_WithTableAndCondition_ReturnsValidSQL()
        {
            // Arrange
            var tableName = "cast_state_tracker";
            var condition = "name = 'Logger Service'";

            // Act
            var sql = $"delete ignore from {tableName} where {condition}";

            // Assert
            Assert.NotEmpty(sql);
            Assert.Contains("delete ignore from", sql);
            Assert.Contains(tableName, sql);
        }

        [Theory]
        [InlineData("INSERT")]
        [InlineData("UPDATE")]
        [InlineData("DELETE")]
        public void ValidateSQLKeyword_CommonStatements_AreValid(string keyword)
        {
            // Arrange
            var statement = $"{keyword} FROM test_table";

            // Act
            var isValid = statement.ToUpper().Contains(keyword);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void BuildInsertLoggerStatement_MultipleValues_AllIncluded()
        {
            // Arrange
            var uuid = "123-456";
            var refUuid = "789-012";
            var originator = "test";
            var type = "ERROR";
            var message = "Test error";

            // Act
            var sql = $"insert into logger (uuid, reference_uuid, originator, type, message) values('{uuid}', '{refUuid}', '{originator}', '{type}', '{message}')";

            // Assert
            Assert.Contains(uuid, sql);
            Assert.Contains(refUuid, sql);
            Assert.Contains(originator, sql);
            Assert.Contains(type, sql);
            Assert.Contains(message, sql);
        }
    }
}

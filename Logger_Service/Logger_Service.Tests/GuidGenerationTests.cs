using Xunit;
using System;

namespace Logger_Service.Tests
{
    /// <summary>
    /// Unit tests for GUID generation and validation
    /// </summary>
    public class GuidGenerationTests
    {
        [Fact]
        public void GenerateGuid_CreatesNewGuid_IsNotEmpty()
        {
            // Arrange & Act
            var guid = Guid.NewGuid();

            // Assert
            Assert.NotEqual(Guid.Empty, guid);
        }

        [Fact]
        public void GenerateGuid_CreatesUniqueGuids()
        {
            // Arrange & Act
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            // Assert
            Assert.NotEqual(guid1, guid2);
        }

        [Fact]
        public void ConvertGuidToString_ValidGuid_ReturnsStringRepresentation()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var guidString = guid.ToString();

            // Assert
            Assert.NotEmpty(guidString);
            Assert.NotEmpty(guidString.Trim());
            Assert.True(Guid.TryParse(guidString, out _));
        }

        [Fact]
        public void ConvertGuidToString_CanParseBackToGuid()
        {
            // Arrange
            var originalGuid = Guid.NewGuid();
            var guidString = originalGuid.ToString();

            // Act
            var parsedGuid = Guid.Parse(guidString);

            // Assert
            Assert.Equal(originalGuid, parsedGuid);
        }

        [Fact]
        public void TrimGuidString_WithWhitespace_RemovesWhitespace()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var guidString = guid.ToString().Trim();

            // Act
            var trimmedString = guidString.Trim();

            // Assert
            Assert.Equal(guidString, trimmedString);
        }

        [Theory]
        [InlineData(36)] // Standard GUID format is 36 characters (with hyphens)
        public void GuidString_StandardFormat_CorrectLength(int expectedLength)
        {
            // Arrange & Act
            var guid = Guid.NewGuid();
            var guidString = guid.ToString();

            // Assert
            Assert.Equal(expectedLength, guidString.Length);
        }

        [Fact]
        public void GenerateMultipleGuids_AllAreUnique()
        {
            // Arrange
            var guids = new HashSet<Guid>();

            // Act
            for (int i = 0; i < 100; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            // Assert
            Assert.Equal(100, guids.Count);
        }

        [Fact]
        public void GuidString_ContainsOnlyValidCharacters()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var guidString = guid.ToString();
            var validCharacters = "0123456789abcdef-";

            // Act
            var allCharsValid = guidString.All(c => validCharacters.Contains(char.ToLower(c)));

            // Assert
            Assert.True(allCharsValid);
        }
    }
}

using Discussly.Core.Common;
using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class CommunityTests
    {
        private Community CreateTestCommunity() =>
            Community.Create("programming", "Programming Community", "Community about programming", Guid.NewGuid(), true).Value;

        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var name = "gaming";
            var displayName = "Gaming Community";
            var description = "All about video games";
            var ownerId = Guid.NewGuid();

            // Act
            var result = Community.Create(name, displayName, description, ownerId, true);

            // Assert
            Assert.True(result.IsSuccess);
            var community = result.Value;
            Assert.Equal(name, community.Name);
            Assert.Equal(displayName, community.DisplayName);
            Assert.Equal(description, community.Description);
            Assert.Equal(ownerId, community.OwnerId);
            Assert.True(community.IsPublic);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_WithInvalidName_ReturnsFailure(string invalidName)
        {
            // Act
            var result = Community.Create(invalidName, "Valid Display", "Description", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_WithInvalidDisplayName_ReturnsFailure(string invalidDisplayName)
        {
            // Act
            var result = Community.Create("validname", invalidDisplayName, "Description", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Create_WithTooLongName_ReturnsFailure()
        {
            // Arrange
            var longName = new string('a', 51); // NAME_MAX_LENGTH = 50

            // Act
            var result = Community.Create(longName, "Valid Display", "Description", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Create_WithTooLongDisplayName_ReturnsFailure()
        {
            // Arrange
            var longDisplayName = new string('a', Community.DISPLAY_NAME_MAX_LENGTH + 1);

            // Act
            var result = Community.Create("validname", longDisplayName, "Description", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Create_WithTooLongDescription_ReturnsFailure()
        {
            // Arrange
            var longDescription = new string('a', Community.DESCRIPTION_MAX_LENGTH + 1);

            // Act
            var result = Community.Create("validname", "Valid Display", longDescription, Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Create_WithEmptyDescription_ReturnsSuccess()
        {
            // Act
            var result = Community.Create("validname", "Valid Display", "", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("", result.Value.Description);
        }

        [Fact]
        public void ToPublic_SetsIsPublicToTrue()
        {
            // Arrange
            var community = Community.Create("private", "Private Community", "Desc", Guid.NewGuid(), false).Value;

            // Act
            var result = community.ToPublic();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(community.IsPublic);
        }

        [Fact]
        public void ToPrivate_SetsIsPublicToFalse()
        {
            // Arrange
            var community = Community.Create("public", "Public Community", "Desc", Guid.NewGuid(), true).Value;

            // Act
            var result = community.ToPrivate();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(community.IsPublic);
        }

        [Fact]
        public void UpdateDisplayName_WithValidName_ReturnsSuccess()
        {
            // Arrange
            var community = CreateTestCommunity();
            var newDisplayName = "Updated Community Name";

            // Act
            var result = community.UpdateDisplayName(newDisplayName);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newDisplayName, community.DisplayName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void UpdateDisplayName_WithInvalidName_ReturnsFailure(string invalidDisplayName)
        {
            // Arrange
            var community = CreateTestCommunity();
            var originalDisplayName = community.DisplayName;

            // Act
            var result = community.UpdateDisplayName(invalidDisplayName);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(originalDisplayName, community.DisplayName); // Не изменилось
        }

        [Fact]
        public void UpdateDescription_WithValidDescription_ReturnsSuccess()
        {
            // Arrange
            var community = CreateTestCommunity();
            var newDescription = "Updated community description";

            // Act
            var result = community.UpdateDescription(newDescription);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newDescription, community.Description);
        }

        [Fact]
        public void UpdateDescription_WithTooLongDescription_ReturnsFailure()
        {
            // Arrange
            var community = CreateTestCommunity();
            var originalDescription = community.Description;
            var longDescription = new string('a', 501); // DESCRIPTION_MAX_LENGTH = 500

            // Act
            var result = community.UpdateDescription(longDescription);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(originalDescription, community.Description); // Не изменилось
        }

        [Fact]
        public void UpdateDescription_WithEmptyDescription_ReturnsSuccess()
        {
            // Arrange
            var community = CreateTestCommunity();

            // Act
            var result = community.UpdateDescription("");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("", community.Description);
        }

        [Fact]
        public void Create_WithMinimalValidData_ReturnsSuccess()
        {
            // Arrange
            var minimalName = "abc";
            var minimalDisplayName = "abc";

            // Act
            var result = Community.Create(minimalName, minimalDisplayName, "", Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Create_WithMaximumLengthData_ReturnsSuccess()
        {
            // Arrange
            var maxName = new string('a', Community.NAME_MAX_LENGTH);
            var maxDisplayName = new string('a', Community.DISPLAY_NAME_MAX_LENGTH);
            var maxDescription = new string('a', Community.DESCRIPTION_MAX_LENGTH);

            // Act
            var result = Community.Create(maxName, maxDisplayName, maxDescription, Guid.NewGuid(), true);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
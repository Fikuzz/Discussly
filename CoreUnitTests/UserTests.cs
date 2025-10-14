using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class UserTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            var result = User.Create("john_doe", "john@test.com", "hash");
            Assert.True(result.IsSuccess);
            Assert.Equal("john_doe", result.Value.Username);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")] // слишком короткий
        [InlineData(null)]
        public void Create_WithInvalidUsername_ReturnsFailure(string username)
        {
            var result = User.Create(username, "test@email.com", "hash");
            Assert.True(result.IsFailure);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("missing.at.sign")]
        [InlineData("")]
        public void Create_WithInvalidEmail_ReturnsFailure(string email)
        {
            var result = User.Create("validuser", email, "hash");
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void UpdateUsername_WithValidData_ReturnsSuccess()
        {
            var user = User.Create("oldname", "test@email.com", "hash").Value;
            var result = user.UpdateUsername("newname");
            Assert.True(result.IsSuccess);
            Assert.Equal("newname", user.Username);
        }

        [Fact]
        public void UpdateKarma_IncreasesValue()
        {
            var user = User.Create("user", "test@email.com", "hash").Value;
            user.UpdateKarma(5);
            Assert.Equal(5, user.Karma);
        }

        [Fact]
        public void SoftDeleteUser_ReturnsSuccess()
        {
            // Arrange
            var user = User.Create("user", "test@email.com", "hash", "avatar.jpg").Value;
            var initialIsDeleted = user.IsDeleted;
            var initialDeletedAt = user.DeletedAt;

            // Act
            user.MarkAsDeleted();

            // Assert
            Assert.False(initialIsDeleted);
            Assert.True(user.IsDeleted);
            Assert.Null(initialDeletedAt);

            Assert.NotNull(user.DeletedAt);
            Assert.InRange(user.DeletedAt.Value,
                DateTime.UtcNow.AddSeconds(-2),
                DateTime.UtcNow.AddSeconds(2));
        }

        [Fact]
        public void RestoreUser_ReturnsSuccess()
        {
            // Arrange
            var user = User.Create("user", "test@email.com", "hash", "avatar.jpg").Value;

            // Act
            user.MarkAsDeleted();
            user.Restore();

            // Assert
            Assert.False(user.IsDeleted);
            Assert.Null(user.DeletedAt);
        }
    }
}

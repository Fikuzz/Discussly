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
        public void BanUser_SetsIsBannedTrue()
        {
            var user = User.Create("user", "test@email.com", "hash").Value;
            user.BanUser();
            Assert.True(user.IsBanned);
        }

        [Fact]
        public void UnbanUser_SetsIsBannedFalse()
        {
            var user = User.Create("user", "test@email.com", "hash").Value;
            user.BanUser();
            user.UnbanUser();
            Assert.False(user.IsBanned);
        }

        [Fact]
        public void UpdateAvatar_WithValidData_ReturnsSuccess()
        {
            var url = "https://avatars.githubusercontent.com/u/53544253?v=4";
            var user = User.Create("user", "test@email.com", "hash").Value;

            var result = user.UpdateAvatar(url);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.AvatarUrl, url);
        }

        [Theory]
        [InlineData("invalid-url")]
        [InlineData("htt://wrong-protocol.com")]
        [InlineData("just text")]
        public void UpdateAvatar_WithInvalidUrlFormat_ReturnsFailure(string invalidUrl)
        {
            var originalAvatar = "https://avatars.githubusercontent.com/u/53544253?v=4";
            var user = User.Create("user", "test@email.com", "hash", originalAvatar).Value;

            var result = user.UpdateAvatar(invalidUrl);

            var expectedErrors = new[]
            {
                "Invalid avatar URL format",
                "Avatar URL must use HTTP or HTTPS protocol"
            };
            Assert.True(result.IsFailure);
            Assert.Contains(expectedErrors, error => result.Error.Contains(error));
            Assert.Equal(originalAvatar, user.AvatarUrl);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void UpdateAvatar_WithEmptyOrNull_SetsAvatarToNull(string emptyUrl)
        {
            var originalAvatar = "https://avatars.githubusercontent.com/u/53544253?v=4";
            var user = User.Create("user", "test@email.com", "hash", originalAvatar).Value;

            var result = user.UpdateAvatar(emptyUrl);

            Assert.True(result.IsSuccess);
            Assert.Null(user.AvatarUrl);
        }

        [Fact]
        public void UpdateAvatar_WithValidUrl_ReturnsSuccess()
        {
            var user = User.Create("user", "test@email.com", "hash", "old-avatar.jpg").Value;
            var newAvatar = "https://cdn.discussly.com/avatars/new-avatar.png";

            var result = user.UpdateAvatar(newAvatar);

            Assert.True(result.IsSuccess);
            Assert.Equal(newAvatar, user.AvatarUrl);
        }
    }
}

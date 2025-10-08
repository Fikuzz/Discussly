using Discussly.Core.Commons;
using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class PostVoteTests
    {
        [Theory]
        [InlineData(VoteType.Downvote)]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Neutral)]
        public void Create_WithValidVoteType_ReturnsSuccess(VoteType voteType)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();

            // Act
            var result = PostVote.Create(userId, postId, voteType);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(userId, result.Value.UserId);
            Assert.Equal(postId, result.Value.PostId);
            Assert.Equal(voteType, result.Value.VoteType);
        }

        [Theory]
        [InlineData(VoteType.Downvote)]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Neutral)]
        public void UpdateVote_WithValidVoteType_ReturnsSuccess(VoteType newVoteType)
        {
            // Arrange
            var initialVoteType = newVoteType == VoteType.Neutral ? VoteType.Downvote : VoteType.Neutral;
            var postVote = PostVote.Create(Guid.NewGuid(), Guid.NewGuid(), initialVoteType).Value;

            // Act
            var result = postVote.UpdateVote(newVoteType);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newVoteType, postVote.VoteType);
        }

        [Fact]
        public void UpdateVote_ToSameType_StillReturnsSuccess()
        {
            // Arrange
            var postVote = PostVote.Create(Guid.NewGuid(), Guid.NewGuid(), VoteType.Upvote).Value;

            // Act
            var result = postVote.UpdateVote(VoteType.Upvote);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(VoteType.Upvote, postVote.VoteType);
        }
    }
}

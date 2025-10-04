using Discussly.Core.Common;
using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class CommentVoteTests
    {
        [Theory]
        [InlineData(VoteType.Downvote)]
        [InlineData(VoteType.Upvote)]
        [InlineData(VoteType.Neutral)]
        public void Create_WithValidVoteType_ReturnsSuccess(VoteType voteType)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            // Act
            var result = CommentVote.Create(userId, commentId, voteType);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(userId, result.Value.UserId);
            Assert.Equal(commentId, result.Value.CommentId);
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
            var commentVote = CommentVote.Create(Guid.NewGuid(), Guid.NewGuid(), initialVoteType).Value;

            // Act
            var result = commentVote.UpdateVote(newVoteType);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newVoteType, commentVote.VoteType);
        }

        [Fact]
        public void UpdateVote_ToSameType_StillReturnsSuccess()
        {
            // Arrange
            var commentVote = CommentVote.Create(Guid.NewGuid(), Guid.NewGuid(), VoteType.Upvote).Value;

            // Act
            var result = commentVote.UpdateVote(VoteType.Upvote);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(VoteType.Upvote, commentVote.VoteType);
        }
    }
}
using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class CommentTests
    {

        private Comment _comment;

        public CommentTests()
        {
            _comment = Comment.Create("This is a valid comment content", Guid.NewGuid(), Guid.NewGuid(), null).Value;
        }

        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var content = "This is a valid comment content";
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();

            // Act
            var result = Comment.Create(content, authorId, postId, null);

            // Assert
            Assert.True(result.IsSuccess);
            var comment = result.Value;
            Assert.Equal(content, comment.ContentText);
            Assert.Equal(authorId, comment.AuthorId);
            Assert.Equal(postId, comment.PostId);
            Assert.Null(comment.ParentCommentId);
            Assert.Equal(0, comment.Score);
        }

        [Fact]
        public void Create_WithParentComment_CreatesReply()
        {
            // Arrange
            var parentCommentId = Guid.NewGuid();

            // Act
            var result = Comment.Create("Reply comment", Guid.NewGuid(), Guid.NewGuid(), parentCommentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(parentCommentId, result.Value.ParentCommentId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_WithEmptyContent_ReturnsFailure(string invalidContent)
        {
            // Act
            var result = Comment.Create(invalidContent, Guid.NewGuid(), Guid.NewGuid(), null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("cannot be empty", result.Error);
        }

        [Fact]
        public void Create_WithTooLongContent_ReturnsFailure()
        {
            // Arrange
            var longContent = new string('a', 1001); // Предполагая MAX_LENGTH = 1000

            // Act
            var result = Comment.Create(longContent, Guid.NewGuid(), Guid.NewGuid(), null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("must not exceed", result.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void UpdateContentText_WithInvalidContent_ReturnsFailure(string invalidContent)
        {
            // Arrange
            var originalContent = "Original content";
            var comment = Comment.Create(originalContent, Guid.NewGuid(), Guid.NewGuid(), null).Value;

            // Act
            var result = comment.UpdateContentText(invalidContent);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(originalContent, comment.ContentText); // Контент не изменился
            Assert.False(comment.IsEdited); // Не помечен как отредактированный
        }

        [Fact]
        public void Upvote_IncreasesScore()
        {
            // Arrange
            var comment = Comment.Create("Test comment", Guid.NewGuid(), Guid.NewGuid(), null).Value;
            var initialScore = comment.Score;

            // Act
            comment.Upvote();

            // Assert
            Assert.Equal(initialScore + 1, comment.Score);
            Assert.True(comment.UpdatedAt > comment.CreatedAt); // UpdatedAt обновился
        }

        [Fact]
        public void Downvote_DecreasesScore()
        {
            // Arrange
            var comment = Comment.Create("Test comment", Guid.NewGuid(), Guid.NewGuid(), null).Value;
            var initialScore = comment.Score;

            // Act
            comment.Downvote();

            // Assert
            Assert.Equal(initialScore - 1, comment.Score);
            Assert.True(comment.UpdatedAt > comment.CreatedAt);
        }

        [Fact]
        public void MultipleVotes_ChangeScoreCorrectly()
        {
            // Arrange
            var comment = Comment.Create("Test comment", Guid.NewGuid(), Guid.NewGuid(), null).Value;

            // Act
            comment.Upvote();  // +1
            comment.Upvote();  // +1
            comment.Downvote(); // -1
            comment.Upvote();  // +1

            // Assert
            Assert.Equal(2, comment.Score); // 1+1-1+1 = 2
        }

        [Fact]
        public void IsEdited_WhenCreated_ReturnsFalse()
        {
            // Arrange & Act
            var comment = Comment.Create("Test comment", Guid.NewGuid(), Guid.NewGuid(), null).Value;

            // Assert
            Assert.False(comment.IsEdited);
        }

        [Fact]
        public void Create_WithMinimalContent_ReturnsSuccess()
        {
            // Arrange
            var minimalContent = "a"; // Минимальная длина

            // Act
            var result = Comment.Create(minimalContent, Guid.NewGuid(), Guid.NewGuid(), null);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Create_WithMaximumContent_ReturnsSuccess()
        {
            // Arrange
            var maxContent = new string('a', 1000); // Максимальная длина

            // Act
            var result = Comment.Create(maxContent, Guid.NewGuid(), Guid.NewGuid(), null);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
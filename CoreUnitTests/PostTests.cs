using Discussly.Core.Entities;


namespace Discussly.Core.UnitTests
{
    public class PostTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            string title = "My first post";
            string contentText = "Its my first post which created spicially for testing post entity with Unit Test!";
            
            var result = Post.Create(title, contentText, Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.IsSuccess);
            var post = result.Value;
            Assert.Equal(post.Title, title);
            Assert.Equal(post.ContentText, contentText);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithEmptyContent_ReturnsSuccess(string content)
        {
            string title = "My first post";

            var result = Post.Create(title, content, Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.IsSuccess);
            var post = result.Value;
            Assert.Equal(post.Title, title);
            Assert.Equal(post.ContentText, content ?? "");
        }

        [Theory]
        [InlineData ("")]
        [InlineData (" ")]
        [InlineData (null)]
        public void Create_WithEmptyTitle_ReturnsFailure(string title)
        {
            var result = Post.Create(title, "some text", Guid.NewGuid(), Guid.NewGuid());
            
            Assert.True(result.IsFailure);
            Assert.Contains("Title cannot be empty", result.Error);
        }

        [Fact]
        public void Create_WithTooLongTitle_ReturnsFailure()
        {
            var title = new string('a', Post.TITLE_MAX_LENGTH+1);
            var result = Post.Create(title, "some text", Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Contains($"Title must not exceed {Post.TITLE_MAX_LENGTH} characters", result.Error);
        }

        [Fact]
        public void Create_WithTooLongContent_ReturnsFailure()
        {
            var content = new string('a', Post.CONTENT_MAX_LENGTH + 1);
            var result = Post.Create("title", content, Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.Contains($"Content must not exceed {Post.CONTENT_MAX_LENGTH} characters", result.Error);
        }

        [Fact]
        public void Update_WithValidTitle_ReturnsSuccess()
        {
            var title = "new title";

            var post = Post.Create("old title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var result = post.UpdateTitle(title);

            Assert.True(result.IsSuccess);
            Assert.Equal(title, post.Title);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Update_WithEmptyTitle_ReturnsFailure(string title)
        {
            var oldTitle = "old title";
            var post = Post.Create(oldTitle, "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var result = post.UpdateTitle(title);

            Assert.True(result.IsFailure);
            Assert.Equal(oldTitle, post.Title);
        }

        [Fact]
        public void Update_WithValidContent_ReturnsSuccess()
        {
            var content = "new text";

            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var result = post.UpdateContent(content);

            Assert.True(result.IsSuccess);
            Assert.Equal(content, post.ContentText);
        }

        [Fact]
        public void Update_WithTooLongContent_ReturnsFailure()
        {
            var oldContent = "some text";
            var content = new string('a', Post.CONTENT_MAX_LENGTH+1);
            var post = Post.Create("title", oldContent, Guid.NewGuid(), Guid.NewGuid()).Value;

            var result = post.UpdateContent(content);

            Assert.True(result.IsFailure);
            Assert.Equal(oldContent, post.ContentText);
        }

        [Fact]
        public void Update_WithValidScore_ReturnsSuccess()
        {
            var score = 4;
            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var result = post.UpdateScore(score);

            Assert.True(result.IsSuccess);
            Assert.Equal(score, post.Score);
        }

        [Fact]
        public void Upvote_ReturnsSuccess()
        {
            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var voteBefore = post.Score;

            post.Upvote();

            Assert.Equal(voteBefore, post.Score - 1);
        }

        [Fact]
        public void Downvote_ReturnsSuccess()
        {
            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var voteBefore = post.Score;

            post.Downvote();

            Assert.Equal(voteBefore, post.Score + 1);
        }

        [Fact]
        public void AddComment_ReturnsSuccess()
        {
            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            var commentsBefore = post.CommentCount;

            post.AddComment();

            Assert.Equal(commentsBefore, post.CommentCount - 1);
        }

        [Fact]
        public void RemoveComment_ReturnsSuccess()
        {
            var post = Post.Create("title", "some text", Guid.NewGuid(), Guid.NewGuid()).Value;

            post.AddComment();

            var commentsBefore = post.CommentCount;

            post.RemoveComment();

            Assert.Equal(commentsBefore, post.CommentCount + 1);
        }

        [Fact]
        public void Create_WithMinimalTitle_ReturnsSuccess()
        {
            var title = "abc";
            var result = Post.Create(title, "content", Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void RemoveComment_WhenZeroComments_DoesNothing()
        {
            var post = Post.Create("title", "content", Guid.NewGuid(), Guid.NewGuid()).Value;
            var initialCount = post.CommentCount;

            post.RemoveComment(); // Попытка удалить когда 0 комментариев

            Assert.Equal(initialCount, post.CommentCount); // Count не должен стать отрицательным
        }
    }
}

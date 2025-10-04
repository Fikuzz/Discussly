using Discussly.Core.Common;
namespace Discussly.Core.Entities
{
    public class Post
    {
        public const int TITLE_MIN_LENGTH = 3;
        public const int TITLE_MAX_LENGTH = 300;
        public const int CONTENT_MAX_LENGTH = 40000;

        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string ContentText { get; private set; } = string.Empty;
        public Guid AuthorId { get; private set; }
        public Guid CommunityId { get; private set; }
        public int Score { get; private set; }
        public int CommentCount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // НАВИГАЦИОННЫЕ СВОЙСТВА:
        public virtual ICollection<PostMediaAttachment> MediaAttachments { get; private set; } = new List<PostMediaAttachment>();
        public virtual ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        private Post() { }
        public static Result<Post> Create(string title, string contentText, Guid authorId, Guid communityId)
        {
            var validationResult = ValidateTitle(title)
                .Combine(ValidateContent(contentText));

            if (validationResult.IsFailure)
                return Result<Post>.Failure(validationResult.Error);

            var post = new Post()
            {
                Id = Guid.NewGuid(),
                Title = title,
                ContentText = contentText ?? "",
                AuthorId = authorId,
                CommunityId = communityId,
                Score = 0,
                CommentCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return Result.Success(post);
        }

        private static Result ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure("Title cannot be empty");
            if (title.Length < TITLE_MIN_LENGTH)
                return Result.Failure($"Title must be at least {TITLE_MIN_LENGTH} characters");
            if (title.Length > TITLE_MAX_LENGTH)
                return Result.Failure($"Title must not exceed {TITLE_MAX_LENGTH} characters");

            return Result.Success();
        }

        private static Result ValidateContent(string content)
        {
            if (content?.Length > CONTENT_MAX_LENGTH)
                return Result.Failure($"Content must not exceed {CONTENT_MAX_LENGTH} characters");
            return Result.Success();
        }

        public Result UpdateTitle(string title)
        {
            var validationResult = ValidateTitle(title);
            if (validationResult.IsFailure)
                return validationResult;

            Title = title;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result UpdateContent(string content)
        {
            var validationResult = ValidateContent(content);
            if (validationResult.IsFailure)
                return validationResult;

            ContentText = content;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result UpdateScore(int newScore)
        {
            Score = newScore;
            return Result.Success();
        }
        public void Upvote()
        {
            Score++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Downvote()
        {
            Score--;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddComment()
        {
            CommentCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveComment()
        {
            if (CommentCount > 0)
                CommentCount--;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsEdited => UpdatedAt > CreatedAt.AddSeconds(1);
    }
}

using Discussly.Core.Common;

namespace Discussly.Core.Entities
{
    public class Comment
    {
        public const int CONTENT_MIN_LENGTH = 1;
        public const int CONTENT_MAX_LENGTH = 1000;

        public Guid Id { get; private set; }
        public string ContentText { get; private set; } = string.Empty;
        public Guid AuthorId { get; private set; }
        public Guid PostId { get; private set; }
        public Guid? ParentCommentId { get; private set; }
        public int Score { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // НАВИГАЦИОННЫЕ СВОЙСТВА:
        public virtual ICollection<CommentMediaAttachment> MediaAttachments { get; private set; } = new List<CommentMediaAttachment>();
        public virtual ICollection<Comment> Replies { get; private set; } = new List<Comment>();

        private Comment() { }
        public static Result<Comment> Create(string contentText, Guid authorId, Guid postId, Guid? parentCommentId)
        {
            var validateResult = ValidateContentText(contentText);

            if (validateResult.IsFailure)
                return Result<Comment>.Failure(validateResult.Error);

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                ContentText = contentText.Trim(),
                AuthorId = authorId,
                PostId = postId,
                ParentCommentId = parentCommentId,
                Score = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return Result.Success(comment);
        }

        private static Result ValidateContentText(string contentText)
        {
            if (string.IsNullOrWhiteSpace(contentText))
                return Result.Failure("Comment content cannot be empty");

            if (contentText.Length < CONTENT_MIN_LENGTH)
                return Result.Failure($"Comment must be at least {CONTENT_MIN_LENGTH} character");

            if (contentText.Length > CONTENT_MAX_LENGTH)
                return Result.Failure($"Comment must not exceed {CONTENT_MAX_LENGTH} characters");

            return Result.Success();
        }

        public Result UpdateContentText(string contentText)
        {
            var validateResult = ValidateContentText(contentText);
            if (validateResult.IsFailure)
                return validateResult;

            ContentText = contentText.Trim();
            UpdatedAt = DateTime.UtcNow;

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

        public bool IsEdited => UpdatedAt > CreatedAt.AddSeconds(1);
    }
}

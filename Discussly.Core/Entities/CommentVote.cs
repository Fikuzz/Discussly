using Discussly.Core.Common;

namespace Discussly.Core.Entities
{
    public class CommentVote
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid CommentId { get; private set; }
        public VoteType VoteType { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private CommentVote() { }

        public static Result<CommentVote> Create(Guid userId, Guid postId, VoteType voteType)
        {
            var commentVote = new CommentVote()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CommentId = postId,
                VoteType = voteType,
                CreatedAt = DateTime.UtcNow
            };

            return Result.Success(commentVote);
        }

        public Result UpdateVote(VoteType voteType)
        {
            VoteType = voteType;
            CreatedAt = DateTime.UtcNow;

            return Result.Success();
        }
    }
}

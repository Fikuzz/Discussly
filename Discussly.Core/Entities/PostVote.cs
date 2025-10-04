using Discussly.Core.Common;

namespace Discussly.Core.Entities
{
    public class PostVote
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid PostId { get; private set; }
        public VoteType VoteType { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private PostVote() { }

        public static Result<PostVote> Create(Guid userId, Guid postId, VoteType voteType)
        {
            var postVote = new PostVote()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PostId = postId,
                VoteType = voteType,
                CreatedAt = DateTime.UtcNow
            };

            return Result.Success(postVote);
        }

        public Result UpdateVote(VoteType voteType)
        {
            VoteType = voteType;
            CreatedAt = DateTime.UtcNow;

            return Result.Success();
        }
    }
}

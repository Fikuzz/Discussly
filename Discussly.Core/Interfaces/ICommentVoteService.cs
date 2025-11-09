using Discussly.Core.Commons;

namespace Discussly.Core.Interfaces
{
    public interface ICommentVoteService
    {
        Task<Result> VoteAsync(Guid id, VoteType voteType, CancellationToken cancellationToken);

        Task<Result<VoteType>> GetVoteType(Guid id, CancellationToken cancellationToken);
    }
}

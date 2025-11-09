using Discussly.Core.Commons;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class PostVoteService : IPostVoteService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<PostVoteService> _logger;

        public PostVoteService(IUserContext userContext, IDiscusslyDbContext context, ILogger<PostVoteService> logger)
        {
            _userContext = userContext;
            _context = context;
            _logger = logger;
        }

        public async Task<Result> VoteAsync(Guid postId, VoteType voteType, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result.Failure("Not authenticated");

                var userId = _userContext.UserId.Value;

                var existingVote = await _context.PostVotes
                    .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId, cancellationToken);

                if (existingVote == null)
                {
                    var result = PostVote.Create(userId, postId, voteType);
                    if (result.IsFailure)
                        return Result.Failure(result.Error);

                    await _context.AddAsync(result.Value);
                }
                else
                {
                    if (existingVote.VoteType == voteType)
                    {
                        return Result.Success();
                    }
                    else
                    {
                        existingVote.UpdateVote(voteType);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Vote was cancelled for post {PostId}", postId);
                return Result.Failure("Vote was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when voting for post {PostId}", postId);
                return Result.Failure("Error when voting");
            }
        }

        public async Task<Result<VoteType>> GetVoteType(Guid postId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result<VoteType>.Failure("Not authenticated");

                var userId = _userContext.UserId.Value;

                var vote = await _context.PostVotes
                    .Where(v => v.PostId == postId && v.UserId == userId)
                    .Select(v => v.VoteType)
                    .FirstOrDefaultAsync(cancellationToken);

                return Result.Success(vote);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Get vote was cancelled for post {PostId}", postId);
                return Result<VoteType>.Failure("Vote check was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting vote for post {PostId}", postId);
                return Result<VoteType>.Failure("Error when getting vote");
            }
        }
    }
}

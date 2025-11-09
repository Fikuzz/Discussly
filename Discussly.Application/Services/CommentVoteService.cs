using Discussly.Core.Commons;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Application.Services
{
    public class CommentVoteService : ICommentVoteService
    {
        private readonly IDiscusslyDbContext _context;
        private readonly IUserContext _userContext;
        private readonly ILogger<CommentVoteService> _logger;
        public CommentVoteService(IDiscusslyDbContext context, IUserContext userContext, ILogger<CommentVoteService> logger)
        {
            _context = context;
            _userContext = userContext;
            _logger = logger;
        }
        public async Task<Result<VoteType>> GetVoteType(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result<VoteType>.Failure("Not authenticated");

                var userId = _userContext.UserId.Value;

                var vote = await _context.CommentVotes
                    .Where(v => v.CommentId == id && v.UserId == userId)
                    .Select(v => v.VoteType)
                    .FirstOrDefaultAsync(cancellationToken);

                return Result.Success(vote);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Get vote was cancelled for comment {id}", id);
                return Result<VoteType>.Failure("Vote check was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting vote for comment {id}", id);
                return Result<VoteType>.Failure("Error when getting vote");
            }
        }

        public async Task<Result> VoteAsync(Guid id, VoteType voteType, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result.Failure("Not authenticated");

                var userId = _userContext.UserId.Value;

                var existingVote = await _context.CommentVotes
                    .FirstOrDefaultAsync(v => v.CommentId == id && v.UserId == userId, cancellationToken);

                if (existingVote == null)
                {
                    var result = CommentVote.Create(userId, id, voteType);
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
                _logger.LogInformation("Vote was cancelled for comment {id}", id);
                return Result.Failure("Vote was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when voting for comment {id}", id);
                return Result.Failure("Error when voting");
            }
        }
    }
}

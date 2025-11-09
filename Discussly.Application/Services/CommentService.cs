using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;
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
    public class CommentService : ICommentService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IUserContext userContext, IDiscusslyDbContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddAsync(CreateCommentDto dto, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result<Guid>.Failure("Couldn't get user id");

                var comment = Comment.Create(dto.Text, userId.Value, dto.PostId, dto.CommentId);
                if (comment.IsFailure)
                    return Result<Guid>.Failure(comment.Error);

                await _context.AddAsync(comment.Value);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"User {userId} add comment '{dto.Text}' on post {dto.PostId}");
                return Result.Success(comment.Value.Id);
            }
            catch(OperationCanceledException)
            {
                _logger.LogInformation("Comment retrieval was cancelled");
                return Result<Guid>.Failure("Comment retrieval was cancelled");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error when adding a comment");
                return Result<Guid>.Failure("Error when adding a comment");
            }
        }

        public async Task<Result<ICollection<CommentDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comments = await _context.Comments.Select(x =>
                    new CommentDto()
                    {
                        Id = x.Id,
                        Text = x.ContentText,
                        PostId = x.PostId,
                        Author = new UserDto()
                        {
                            Id = x.Author.Id,
                            Username = x.Author.Username,
                            AvatarFileName = x.Author.AvatarFileName
                        },
                        CreatedAt = x.CreatedAt,
                        CommentCount = x.Replies.Count(),
                        Score = x.Votes.Sum(v => (short)v.VoteType),
                        IsEditing = x.IsEdited
                    }).ToListAsync(cancellationToken);

                return Result<ICollection<CommentDto>>.Success(comments);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Comment retrieval was cancelled");
                return Result<ICollection<CommentDto>>.Failure("Comment retrieval was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when receiving comments");
                return Result<ICollection<CommentDto>>.Failure("Error when receiving comments");
            }
        }

        public async Task<Result<ICollection<CommentDto>>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comments = await _context.Comments
                    .Where(x => x.PostId == postId && x.ParentCommentId == null)
                    .Select(x =>
                    new CommentDto()
                    {
                        Id = x.Id,
                        Text = x.ContentText,
                        PostId = x.PostId,
                        Author = new UserDto()
                        {
                            Id = x.Author.Id,
                            Username = x.Author.Username,
                            AvatarFileName = x.Author.AvatarFileName
                        },
                        CreatedAt = x.CreatedAt,
                        CommentCount = x.Replies.Count(),
                        Score = x.Votes.Sum(v => (short)v.VoteType),
                        IsEditing = x.IsEdited
                    }).ToListAsync(cancellationToken);

                return Result<ICollection<CommentDto>>.Success(comments);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Comment retrieval was cancelled");
                return Result<ICollection<CommentDto>>.Failure("Comment retrieval was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when receiving comments");
                return Result<ICollection<CommentDto>>.Failure("Error when receiving comments");
            }
        }

        public async Task<Result<ICollection<CommentDto>>> GetSubCommentAsync(Guid commentId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comments = await _context.Comments
                    .Where(x => x.ParentCommentId == commentId)
                    .Select(x =>
                    new CommentDto()
                    {
                        Id = x.Id,
                        Text = x.ContentText,
                        PostId = x.PostId,
                        Author = new UserDto()
                        {
                            Id = x.Author.Id,
                            Username = x.Author.Username,
                            AvatarFileName = x.Author.AvatarFileName
                        },
                        CreatedAt = x.CreatedAt,
                        CommentCount = x.Replies.Count(),
                        Score = x.Votes.Sum(v => (short)v.VoteType),
                        IsEditing = x.IsEdited
                    }).ToListAsync(cancellationToken);

                return Result<ICollection<CommentDto>>.Success(comments);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Comment retrieval was cancelled");
                return Result<ICollection<CommentDto>>.Failure("Comment retrieval was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when receiving comments");
                return Result<ICollection<CommentDto>>.Failure("Error when receiving comments");
            }
        }

        public async Task<Result> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated)
                    return Result.Failure("Not authenticated");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result.Failure("Couldn't get user id");

                var deletedCount = await _context.Comments
                    .Where(c => c.Id == id && c.AuthorId == userId.Value)
                    .ExecuteDeleteAsync(cancellationToken);

                if (deletedCount == 0)
                {
                    var commentExists = await _context.Comments
                        .AnyAsync(c => c.Id == id, cancellationToken);

                    return commentExists
                        ? Result.Failure("Not enough rights to delete this comment")
                        : Result.Failure("Comment not found");
                }

                _logger.LogInformation("User {UserId} deleted comment {CommentId}", userId, id);
                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Comment delete was cancelled for comment {CommentId}", id);
                return Result.Failure("Comment delete was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting comment {CommentId}", id);
                return Result.Failure("Error when deleting comment");
            }
        }
    }
}

using Discussly.Application.Interfaces;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly IStorageService _storageService;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IUserContext userContext, IDiscusslyDbContext context, ILogger<CommentService> logger, IStorageService storageService)
        {
            _context = context;
            _userContext = userContext;
            _logger = logger;
            _storageService = storageService;
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

                var commentResult = Comment.Create(dto.Text, userId.Value, dto.PostId, dto.CommentId);
                if (commentResult.IsFailure)
                    return Result<Guid>.Failure(commentResult.Error);

                var comment = commentResult.Value;

                if (dto.Media != null)
                {
                    var mediaInfoResult = await _storageService.SaveFileAsync(comment.Id, dto.Media, Storage.CommentMedia);
                    if (mediaInfoResult.IsSuccess)
                    {
                        comment.UpdateMedia(Path.Combine(mediaInfoResult.Value.FilePath, mediaInfoResult.Value.FileName));
                    }
                }
                
                await _context.AddAsync(comment);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"User {userId} add comment '{dto.Text}' on post {dto.PostId}");
                return Result.Success(comment.Id);
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

                var comments = await _context.Comments
                    .Select(x => new CommentDto()
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
                        IsEditing = x.IsEdited,
                        MediaFileName = x.MediaFileName
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
        public async Task<Result<CommentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comment = await _context.Comments
                    .Select(x => new CommentDto()
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
                        IsEditing = x.IsEdited,
                        MediaFileName = x.MediaFileName
                    })
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comment == null)
                    return Result<CommentDto>.Failure("Comment not found.");

                return Result.Success(comment);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Comment retrieval was cancelled");
                return Result<CommentDto>.Failure("Comment retrieval was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when receiving comment");
                return Result<CommentDto>.Failure("Error when receiving comment");
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
                        IsEditing = x.IsEdited,
                        MediaFileName = x.MediaFileName
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
                        IsEditing = x.IsEdited,
                        MediaFileName = x.MediaFileName
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

                var comments = await _context.Comments
                    .Where(c => c.Id == id)
                    .Include(c => c.Replies)
                    .ThenInclude(c => c.Replies)
                    .ToListAsync();

                var mediaNames = CollectMediaFromCommentTree(comments);
                foreach(var mediaName in mediaNames)
                {
                    _storageService.DeleteFile(mediaName, Storage.CommentMedia, FileType.Image);
                }

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

        private List<string> CollectMediaFromCommentTree(IEnumerable<Comment> comments)
        {
            var result = new List<string>();

            foreach(Comment comment in comments)
            {
                if(!string.IsNullOrEmpty(comment.MediaFileName))
                    result.Add(comment.MediaFileName);

                if(comment.Replies.Any())
                    result.AddRange(
                        CollectMediaFromCommentTree(comment.Replies));
            }

            return result;
        }
    }
}

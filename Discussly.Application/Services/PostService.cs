using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<PostService> _logger;

        public PostService(IUserContext userContext, IDiscusslyDbContext context, ILogger<PostService> logger)
        {
            _context = context;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<Guid>> CreateAsync(CreatePostDto dto, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_userContext.IsAuthenticated)
                return Result<Guid>.Failure("Not authenticated");

            var userId = _userContext.UserId;
            if (userId == null)
                return Result<Guid>.Failure("Couldn't get user id");

            var community = await _context.Communities.FirstOrDefaultAsync(c => c.Id == dto.CommunityId);
            if (community == null)
                return Result<Guid>.Failure("Community not found");
            
            try
            {
                var post = Post.Create(
                    dto.Title,
                    dto.ContentText,
                    userId.Value,
                    dto.CommunityId
                );

                if (post.IsFailure)
                    return Result<Guid>.Failure(post.Error);

                await _context.AddAsync(post.Value);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Post created by user {userId}");
                return Result.Success(post.Value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating a post");
                return Result<Guid>.Failure("Error when creating a post");
            }
        }

        public async Task<Result<ICollection<PostDto>>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var posts = await _context.Posts
                    .Select(p => new PostDto()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        ContentText = p.ContentText,
                        Author = p.Author != null ? new UserDto()
                        {
                            Id = p.Author.Id,
                            Username = p.Author.Username,
                            AvatarFileName = p.Author.AvatarFileName
                        } : null,
                        Community = p.Community != null ? new CommunityDto()
                        {
                            Id = p.Community.Id,
                            DisplayName = p.Community.DisplayName,
                            Description = p.Community.Description,
                            AvatarFileName = p.Community.AvatarFileName,
                            CreatedAt = p.Community.CreatedAt,
                            ParticipantCount = p.Community.Members.Count,
                            PostCount = p.Community.Posts.Count
                        } : null,
                        Score = p.Votes.Sum(v => (int)v.VoteType),
                        CommentCount = p.Comments.Count,
                        CreatedAt = p.CreatedAt,
                        MediaPreviewFileName = p.MediaAttachments
                            .OrderBy(ma => ma.SortOrder)
                            .Select(ma => ma.FileUrl)
                            .FirstOrDefault()
                    })
                    .ToListAsync(cancellationToken);

                return posts != null
                    ? Result<ICollection<PostDto>>.Success(posts)
                    : Result<ICollection<PostDto>>.Failure("Post not found");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Posts retrieval was cancelled");
                return Result<ICollection<PostDto>>.Failure("Operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return Result<ICollection<PostDto>>.Failure("Error getting post");
            }
        }

        public async Task<Result<PostDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var post = await _context.Posts
                    .Where(p => p.Id == id)
                    .Select(p => new PostDto()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        ContentText = p.ContentText,
                        Author = p.Author != null ? new UserDto()
                        {
                            Id = p.Author.Id,
                            Username = p.Author.Username,
                            AvatarFileName = p.Author.AvatarFileName
                        } : null,
                        Community = p.Community != null ? new CommunityDto()
                        {
                            Id = p.Community.Id,
                            DisplayName = p.Community.DisplayName,
                            Description = p.Community.Description,
                            AvatarFileName = p.Community.AvatarFileName,
                            CreatedAt = p.Community.CreatedAt,
                            ParticipantCount = p.Community.Members.Count,
                            PostCount = p.Community.Posts.Count
                        } : null,
                        Score = p.Votes.Sum(v => (int)v.VoteType),
                        CommentCount = p.Comments.Count,
                        CreatedAt = p.CreatedAt,
                        MediaPreviewFileName = p.MediaAttachments
                            .OrderBy(ma => ma.SortOrder)
                            .Select(ma => ma.FileUrl)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return post != null
                    ? Result<PostDto>.Success(post)
                    : Result<PostDto>.Failure("Post not found");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Post retrieval was cancelled for {PostId}", id);
                return Result<PostDto>.Failure("Operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post {PostId}", id);
                return Result<PostDto>.Failure("Error getting post");
            }
        }

        public async Task<Result<ICollection<PostDto>>> GetCommunityPost(Guid communityId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var posts = await _context.Posts
                    .Where(p => p.CommunityId == communityId)
                    .Select(p => new PostDto()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        ContentText = p.ContentText,
                        Author = p.Author != null ? new UserDto()
                        {
                            Id = p.Author.Id,
                            Username = p.Author.Username,
                            AvatarFileName = p.Author.AvatarFileName
                        } : null,
                        Community = null,
                        Score = p.Votes.Sum(v => (int)v.VoteType),
                        CommentCount = p.Comments.Count,
                        CreatedAt = p.CreatedAt,
                        MediaPreviewFileName = p.MediaAttachments
                            .OrderBy(ma => ma.SortOrder)
                            .Select(ma => ma.FileUrl)
                            .FirstOrDefault()
                    }).ToListAsync(cancellationToken);

                return posts != null
                    ? Result<ICollection<PostDto>>.Success(posts)
                    : Result<ICollection<PostDto>>.Failure("Post not found");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Posts retrieval was cancelled");
                return Result<ICollection<PostDto>>.Failure("Operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return Result<ICollection<PostDto>>.Failure("Error getting post");
            }
        }
    }
}

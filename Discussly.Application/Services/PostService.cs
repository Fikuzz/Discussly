using Discussly.Application.Interfaces;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<PostService> _logger;
        private readonly IStorageService _storageService;

        public PostService(IUserContext userContext, IDiscusslyDbContext context, ILogger<PostService> logger, IStorageService storageService)
        {
            _context = context;
            _userContext = userContext;
            _logger = logger;
            _storageService = storageService;
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

                if (dto.MediaFiles != null) {
                    int order = 0;
                    foreach (IFormFile file in dto.MediaFiles)
                    {
                        var fileId = Guid.NewGuid();
                        var result = await _storageService.SaveFileAsync(fileId, file, Storage.PostMedia);

                        if (result.IsFailure)
                        {
                            _logger.LogError(result.Error);
                            continue;
                        }

                        var postMedia = PostMediaAttachment.Create(
                            fileId, 
                            post.Value.Id, 
                            result.Value.FileName, 
                            result.Value.FileType, 
                            result.Value.FilePath, 
                            result.Value.FileSize, 
                            order, 
                            result.Value.Metadata);
                        if (postMedia.IsFailure)
                        {
                            _logger.LogError(postMedia.Error);
                            continue;
                        }
                        order++;
                        await _context.AddAsync(postMedia.Value);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"{order} media files uploaded");
                }

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
                            .Select(ma => Path.Combine(ma.Path, ma.FileName))
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
                            .Select(ma => Path.Combine(ma.Path, ma.FileName))
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
                            .Select(ma => Path.Combine(ma.Path, ma.FileName))
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

        public async Task<Result> DeletePostAsync(Guid postId, CancellationToken cancellationToken)
        {
            if (!_userContext.IsAuthenticated)
                return Result.Failure("User not authenticated.");
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var userId = _userContext.UserId;

                var post = await _context.Posts
                    .Include(x => x.MediaAttachments)
                    .FirstOrDefaultAsync(x => x.Id == postId, cancellationToken);
                if (post == null)
                    return Result.Failure($"post {postId} now found.");

                //TODO: accept for admin and moder
                if (post.AuthorId != userId)
                    return Result.Failure("Not enough rights to delete this post.");

                foreach (PostMediaAttachment media in post.MediaAttachments)
                {
                    var storageResult = _storageService.DeleteFile(media.FileName, Storage.PostMedia, media.FileType);
                    if (storageResult.IsFailure)
                    {
                        _logger.LogWarning($"Error deleting file {media.FileName}: {storageResult.Error}");
                        continue;
                    }
                    _context.Remove(media);
                }

                _context.Remove(post);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Post deleting was canceled.");
                return Result.Failure("Post deleting was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred when deleting the post {postId}.");
                return Result.Failure("Error occurred when deleting the post.");
            }
        }

        public async Task<Result<ICollection<MediaDto>>> GetMedia(Guid postId,  CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var media = await _context.PostMediaAttachments
                    .Where(x => x.PostId == postId)
                    .ToListAsync(cancellationToken);

                var mediaDtos = MediaDto.MapList(media);

                return Result.Success(mediaDtos);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Receiving a media has been canceled.");
                return Result<ICollection<MediaDto>>.Failure("Receiving a media has been canceled.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error receiving post media");
                return Result<ICollection<MediaDto>>.Failure("Error receiving post media");
            }
        }
    }
}

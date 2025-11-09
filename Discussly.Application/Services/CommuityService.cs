using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Discussly.Application.Services
{
    public class CommuityService : ICommuityService
    {
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<CommuityService> _logger;

        public CommuityService(IUserContext userContext, IDiscusslyDbContext context, ILogger<CommuityService> logger)
        {
            _userContext = userContext;
            _context = context;
            _logger = logger;
        }
        
        public async Task<Result<CommunityDto>> GetAsync(Guid communityId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var community = await _context.Communities
                    .Select(c => new CommunityDto
                    {
                        Id = c.Id,
                        DisplayName = c.Name,
                        Description = c.Description,
                        AvatarFileName = c.AvatarFileName,
                        CreatedAt = c.CreatedAt,
                        ParticipantCount = c.Members.Count,
                        PostCount = c.Posts.Count
                    }).FirstOrDefaultAsync(c => c.Id == communityId, cancellationToken);

                if (community == null)
                    return Result<CommunityDto>.Failure("Community not exist");

                return Result.Success(community);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while receiving the community");
                return Result<CommunityDto>.Failure("Error occurred while receiving the community");
            }
        }
        public async Task<Result<ICollection<CommunityDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var communities = await _context.Communities
                    .Select(c => new CommunityDto
                    {
                        Id = c.Id,
                        DisplayName = c.Name,
                        Description = c.Description,
                        AvatarFileName = c.AvatarFileName,
                        CreatedAt = c.CreatedAt,
                        ParticipantCount = c.Members.Count,
                        PostCount = c.Posts.Count
                    }).ToListAsync(cancellationToken);

                if (communities == null)
                    return Result<ICollection<CommunityDto>>.Failure("couldn't get communities");

                return Result<ICollection<CommunityDto>>.Success(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving communities");
                return Result<ICollection<CommunityDto>>.Failure("Error receiving communities");
            }
        }
        public async Task<Result<Guid>> CreateAsync(CommunityCreateRequest createRequest, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!_userContext.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");
                var userId = _userContext.UserId;

                var communityResult = Community.Create(
                    createRequest.Name,
                    createRequest.Name,
                    null,
                    createRequest.Description,
                    userId.Value,
                    createRequest.IsPublic);

                if (communityResult.IsFailure)
                    return Result<Guid>.Failure(communityResult.Error);

                var community = communityResult.Value;

                await _context.AddAsync(community);

                var creatorResult = CommunitySubscription.Create(userId.Value, communityResult.Value.Id);
                if (creatorResult.IsFailure)
                    return Result<Guid>.Failure(creatorResult.Error);

                var creator = creatorResult.Value;
                creator.SetRole(CommunityRoleType.Creaeter);

                await _context.AddAsync(creator);

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(community.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating community");
                return Result<Guid>.Failure("Error occurred while creating community");
            }
        }
        
    }
}

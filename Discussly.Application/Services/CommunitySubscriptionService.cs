using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class CommunitySubscriptionService : ICommunitySubscriptionService
    {
        private readonly IDiscusslyDbContext _context;
        private readonly ILogger<CommunitySubscriptionService> _logger;
        private readonly IUserContext _userContext;

        public CommunitySubscriptionService(IDiscusslyDbContext context, ILogger<CommunitySubscriptionService> logger, IUserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<Result<MemberDto?>> CheckSubscriptionAsync(Guid communityId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated)
                    return Result<MemberDto?>.Failure("Not authenticated");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result<MemberDto?>.Failure("Сouldn't get user id");

                var subscription = await _context.CommunitySubscriptions
                    .FirstOrDefaultAsync(s => 
                    s.UserId == userId && s.CommunityId == communityId,
                    cancellationToken);

                if (subscription == null)
                    return Result<MemberDto?>.Success(null);

                var member = await _context.Users
                    .Select(u => new MemberDto()
                    {
                        User = new UserDto() 
                        {
                            Id = u.Id,
                            AvatarFileName = u.AvatarFileName,
                            Username = u.Username
                        },
                        MemberAt = subscription.SubscribedAt,
                        Role = subscription.Role
                    })
                    .FirstOrDefaultAsync(u => u.User.Id == subscription.UserId, cancellationToken);

                if (member == null)
                    return Result<MemberDto?>.Failure("Couldn't get user information");

                return Result<MemberDto?>.Success(member);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CheckSubscription was cancelled");
                return Result<MemberDto?>.Failure("CheckSubscription was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when check subscription");
                return Result<MemberDto?>.Failure("Error when check subscription");
            }
        }

        public async Task<Result<ICollection<MemberDto>>> CommunitySubsribtionsAsync(Guid communityId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var users = await _context.CommunitySubscriptions
                    .Where(c => c.CommunityId == communityId)
                    .Select(c => new MemberDto() {
                        User = UserDto.Map(c.User),
                        MemberAt = c.SubscribedAt,
                        Role = c.Role
                    })
                    .ToListAsync(cancellationToken);

                return Result<ICollection<MemberDto>>.Success(users);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CommunitySubsribtions was cancelled");
                return Result<ICollection<MemberDto>>.Failure("CommunitySubsribtions was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving community subscribers");
                return Result<ICollection<MemberDto>>.Failure("Error receiving community subscribers");
            }
        }

        public async Task<Result<MemberDto>> SubscribeAsync(Guid communityId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated)
                    return Result<MemberDto>.Failure("Not authenticated");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result<MemberDto>.Failure("Сouldn't get user id");

                var existingSubscription = await _context.CommunitySubscriptions
                    .AnyAsync(s => s.UserId == userId && s.CommunityId == communityId, cancellationToken);

                if (existingSubscription)
                    return Result<MemberDto>.Failure("Already subscribed");

                var subscribe = CommunitySubscription.Create(userId.Value, communityId);

                if (subscribe.IsFailure)
                    return Result<MemberDto>.Failure(subscribe.Error);

                await _context.AddAsync(subscribe.Value);
                await _context.SaveChangesAsync(cancellationToken);

                var member = await _context.Users
                    .Select(u => new MemberDto()
                    {
                        User = new UserDto()
                        {
                            Id = u.Id,
                            AvatarFileName = u.AvatarFileName,
                            Username = u.Username
                        },
                        MemberAt = subscribe.Value.SubscribedAt,
                        Role = subscribe.Value.Role
                    })
                    .FirstOrDefaultAsync(u => u.User.Id == subscribe.Value.UserId, cancellationToken);

                if (member == null)
                    return Result<MemberDto>.Failure("Couldn't get user information");

                return Result.Success(member);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Subscribe was cancelled");
                return Result<MemberDto>.Failure("Subscribe was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when trying to subscribe");
                return Result<MemberDto>.Failure("Error when trying to subscribe");
            }
        }

        public async Task<Result> UnsubscribeAsync(Guid communityId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_userContext.IsAuthenticated)
                    return Result.Failure("Not authenticated");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result.Failure("Сouldn't get user id");

                var subscribe = await _context.CommunitySubscriptions
                    .FirstOrDefaultAsync(s =>
                    s.UserId == userId && s.CommunityId == communityId,
                    cancellationToken);

                if (subscribe == null)
                    return Result.Success();

                if(subscribe.Role == CommunityRoleType.Creaeter)
                    return Result.Failure("Сan't unsubscribe as a creator");

                _context.Remove(subscribe);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UnSubscribe was cancelled");
                return Result.Failure("UnSubscribe was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when trying to unsubscribe");
                return Result.Failure("Error when trying to unsubscribe");
            }
        }

        public async Task<Result<ICollection<CommunityDto>>> UserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var users = await _context.CommunitySubscriptions
                    .Where(c => c.UserId == userId)
                    .Select(c => new CommunityDto()
                    {
                        Id = c.Community.Id,
                        DisplayName = c.Community.DisplayName,
                        Description = c.Community.Description,
                        AvatarFileName = c.Community.AvatarFileName,
                        CreatedAt = c.Community.CreatedAt,
                        ParticipantCount = c.Community.Members.Count,
                        PostCount = c.Community.Posts.Count
                    })
                    .ToListAsync(cancellationToken);

                return Result<ICollection<CommunityDto>>.Success(users);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CommunitySubsribtions was cancelled");
                return Result<ICollection<CommunityDto>>.Failure("CommunitySubsribtions was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving community subscribers");
                return Result<ICollection<CommunityDto>>.Failure("Error receiving community subscribers");
            }
        }
    }
}

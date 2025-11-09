using Discussly.Application.Hubs;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Discussly.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserContext _userContext;
        private readonly IDiscusslyDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IHubContext<UserHub> _userHub;

        public UserService(IDiscusslyDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService, IUserContext userContext, ILogger<UserService> logger, IHubContext<UserHub> userHub)
        {
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _userContext = userContext;
            _userHub = userHub;
        }
        

        // Delete
        public async Task<Result> SoftDeleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_userContext.IsAuthenticated)
                return Result.Failure("Couldn't identify the user");

            var userId = _userContext.UserId;
            if (userId == null)
                return Result.Failure("Couldn't get user ID");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
                return Result.Failure("User not found");

            user.MarkAsDeleted(); // Domain метод для soft delete
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        public async Task<Result> RestoreAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == userId && x.IsDeleted, cancellationToken);

                if (user == null)
                    return Result<User>.Failure("User with this email or username not found.");

                user.Restore();
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user {UserId}", userId);
                return Result.Failure("Error occurred while restoring account");
            }
        }
        public async Task<Result<int>> PurgeDeletedUsersAsync(int daysOld, CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                            return Result<int>.Failure("Authentication required");

                        if (_userContext.Role != RoleType.Admin)
                            return Result<int>.Failure("Insufficient permissions. Admin role required");

                        if (daysOld < 0)
                            return Result<int>.Failure("DaysOld must be positive");

                        var deleteBefore = DateTime.UtcNow.AddDays(-daysOld);

                        var usersToDelete = _context.Users
                            .Where(u => u.IsDeleted && u.DeletedAt.HasValue && u.DeletedAt.Value < deleteBefore);

                        var deletedCount = await usersToDelete.ExecuteDeleteAsync(cancellationToken);

                        _logger.LogInformation($"Permanently deleted {deletedCount} users older than {daysOld} days");

                        return Result<int>.Success(deletedCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error purging deleted users older than {daysOld} days");
                        return Result<int>.Failure("Error occurred while purging deleted users");
                    }
                }

        // Login - Register
        public async Task<Result<AuthResponse>> LoginAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedLogin = request.Login.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedLogin, cancellationToken)
                ?? await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedLogin, cancellationToken);
            if (user == null)
                return Result<AuthResponse>.Failure("User with this email or username not found.");

            if (user.IsDeleted)
                return Result<AuthResponse>.Failure($"Account was deleted on {user.DeletedAt?.ToString("dd.MM.yyyy")}.");

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("Incorect password");

            var token = _jwtService.GenerateToken(user);

            var response = new AuthResponse(UserDto.Map(user), token);
            return Result.Success(response);
        }
        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken))
                return Result<AuthResponse>.Failure("User with this email already exists");

            var result = User.Create(request.Username, request.Email,
                     _passwordHasher.HashPassword(request.Password));
            
            if (result.IsFailure)
                return Result<AuthResponse>.Failure(result.Error);

            cancellationToken.ThrowIfCancellationRequested();

            var user = result.Value;

            await _context.AddAsync(user);
            await _context.SaveChangesAsync(cancellationToken);

            var token = _jwtService.GenerateToken(user);
            return Result.Success(new AuthResponse(UserDto.Map(user), token));
        }
        

        // Role
        public async Task<Result> AssignRoleAsync(Guid userId, RoleType role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!_userContext.IsAuthenticated)
                    return Result.Failure("Authentication required");

                if (_userContext.Role != RoleType.Admin)
                    return Result.Failure("Permission denied. Admin role required");

                var user = await _context.Users.
                    FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                if (user == null)
                    return Result<User>.Failure("User not found.");

                user.AssignRole(role);
                await _context.SaveChangesAsync(cancellationToken);


                _logger.LogInformation($"Admin {_userContext.UserId} assigned role {role} to user {userId}");
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role {role} to user {userId}");
                return Result.Failure("Error occurred while assigning role");
            }
        }

        // Get info
        public async Task<PagedList<User>> GetUsersAsync(UserQuery query, CancellationToken cancellationToken)
        {
            var usersQuery = _context.Users
                .Include(u => u.Bans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Username.Contains(query.Search) ||
                    u.Email.Contains(query.Search));
            }

            if (query.Role.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.Role == query.Role.Value);
            }

            if (query.IsDeleted.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsDeleted == query.IsDeleted.Value);
            }

            if (query.CreatedAfter.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.CreatedAt >= query.CreatedAfter.Value);
            }

            if (query.CreatedBefore.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.CreatedAt <= query.CreatedBefore.Value);
            }

            usersQuery = query.ApplyBanFilters(usersQuery);

            // Сортировка
            usersQuery = query.SortBy.ToLower() switch
            {
                "username" => query.SortDescending
                    ? usersQuery.OrderByDescending(u => u.Username)
                    : usersQuery.OrderBy(u => u.Username),
                "email" => query.SortDescending
                    ? usersQuery.OrderByDescending(u => u.Email)
                    : usersQuery.OrderBy(u => u.Email),
                "createdat" or _ => query.SortDescending
                    ? usersQuery.OrderByDescending(u => u.CreatedAt)
                    : usersQuery.OrderBy(u => u.CreatedAt)
            };

            // Пагинация
            var totalCount = await usersQuery.CountAsync(cancellationToken);
            var users = await usersQuery
                .Skip(query.Skip)
                .Take(query.Take)
                .ToListAsync(cancellationToken);

            return new PagedList<User>(users, totalCount, query.Page, query.PageSize);
        }
        public async Task<Result<UserDto>> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (user == null)
                    return Result<UserDto>.Failure("User not found");

                return Result.Success(UserDto.Map(user));
            }
            catch (Exception ex) 
            {
                return Result<UserDto>.Failure($"Error occurred while receiving user. {ex.Message}");
            }
        }
        public async Task<Result<ProfileDto>> GetProfileAsync(CancellationToken cancellationToken, Guid? userId = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var targetUserId = userId ?? _userContext.UserId;
                if (!targetUserId.HasValue)
                    return Result<ProfileDto>.Failure("User not authenticated");

                var isOwner = userId == null || userId == _userContext.UserId;

                var user = await _context.Users
                    .Where(u => u.Id == targetUserId)
                    .Select(u => new ProfileDto()
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        AvatarFileName = u.AvatarFileName,
                        Karma = u.Posts.Sum(p => p.Votes.Sum(v => (short)v.VoteType)),
                        CreatedAt = u.CreatedAt,
                        PostCount = u.Posts.Count(),
                        CommentCount = u.Comments.Count()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result<ProfileDto>.Failure("User not found");

                var dto = new ProfileDto()
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = isOwner ? user.Email : null,
                    AvatarFileName = user.AvatarFileName,
                    Karma = user.Karma,
                    PostCount = user.PostCount,
                    CommentCount = user.CommentCount,
                    CreatedAt = user.CreatedAt
                };

                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't get profile data");
                return Result<ProfileDto>.Failure("Couldn't get profile data");
            }
        }

        // Ban
        public async Task<Result> BanUserAsync(Guid userId, string reason, int? durationMinutes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result.Failure("Authentication required");

                if (_userContext.Role < RoleType.Moderator)
                    return Result.Failure("Insufficient permissions");

                var user = await _context.Users
                    .Include(u => u.Bans)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                    return Result.Failure("User not found");

                if (user.IsBanned)
                    return Result.Failure("User is already banned");

                if (userId == _userContext.UserId.Value)
                    return Result.Failure("Cannot ban yourself");

                if (user.Role >= RoleType.Moderator && userId != _userContext.UserId.Value)
                    return Result.Failure("Cannot ban other moderators or administrators");

                DateTime? expiresAt = durationMinutes.HasValue
                    ? DateTime.UtcNow.AddMinutes(durationMinutes.Value)
                    : null;

                if (durationMinutes.HasValue && durationMinutes.Value <= 0)
                    return Result.Failure("Ban duration must be positive");

                var ban = Ban.Create(userId, _userContext.UserId.Value, reason, expiresAt);

                await _context.AddAsync(ban);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} banned by {ModeratorId} for {Duration} minutes. Reason: {Reason}",
                    userId, _userContext.UserId, durationMinutes, reason);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", userId);
                return Result.Failure("Error occurred while banning user");
            }
        }
        public async Task<Result> UnbanUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result.Failure("Authentication required");

                if (_userContext.Role < RoleType.Moderator)
                    return Result.Failure("Insufficient permissions");

                var user = await _context.Users
                    .Include(u => u.Bans)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                    return Result.Failure("User not found");

                if (!user.IsBanned)
                    return Result.Failure("User is not banned");

                var activeBan = user.GetActiveBan();

                activeBan.Unban(_userContext.UserId.Value);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} unbanned by {ModeratorId}. Previous ban reason: {BanReason}",
                    userId, _userContext.UserId, activeBan.Reason);

                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", userId);
                return Result.Failure("Error occurred while unbanning user");
            }
        }
        
        // Avatar
        public async Task<Result> UpdateAvatar(string? avatarName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!_userContext.IsAuthenticated)
                    return Result.Failure("Not authenticated");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _userContext.UserId, cancellationToken);
                if (user == null)
                    return Result.Failure("User not found");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result.Failure("Couldn't get user ID");

                user.UpdateAvatar(avatarName);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Avatar updated successly for user {userId}");
                await _userHub.Clients.Group($"user-{userId}").SendAsync("AvatarChanged", avatarName);
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating avatar {avatarName}");
                return Result.Failure("Error occurred while updating avatar");
            }
        }
        public async Task<Result> DeleteAvatar(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!_userContext.IsAuthenticated)
                    return Result.Failure("Not authenticated");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _userContext.UserId, cancellationToken);
                if (user == null)
                    return Result.Failure("User not found");

                var userId = _userContext.UserId;
                if (userId == null)
                    return Result.Failure("Couldn't get user ID");

                user.UpdateAvatar(null);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Avatar deleted successly for user {userId}");
                await _userHub.Clients.Group($"user-{userId}").SendAsync("AvatarChanged", null);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deletind avatar");
                return Result.Failure("Error occurred while deletind avatar");
            }
        }
        public async Task<Result<string?>> GetUserAvatarNameAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Result<string?>.Failure("User not found");

            return Result.Success(user.AvatarFileName);
        }

        // Username
        public async Task<Result> UpdateUsernameAsync(string username, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_userContext.IsAuthenticated == false)
                return Result.Failure("User not Authenticated");

            var userId = _userContext.UserId;
            if (userId == null)
                return Result.Failure("Couldn't get user ID");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                return Result.Failure("User not found");

            var result = user.UpdateUsername(username);

            await _context.SaveChangesAsync(cancellationToken);
            await _userHub.Clients.Group($"user-{userId}").SendAsync("UsernameChanged", username);
            _logger.LogInformation($"Send UsernameChanged message with username:{username} to group: user-{userId}");

            return result;
        }
    }
}

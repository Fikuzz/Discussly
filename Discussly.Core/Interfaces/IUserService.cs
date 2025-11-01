using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;

namespace Discussly.Core.Interfaces
{
    public interface IUserService
    {
        // Delete
        Task<Result> SoftDeleteAsync(CancellationToken cancellationToken);
        Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<int>> PurgeDeletedUsersAsync(int daysOld, CancellationToken cancellationToken);

        //Login - Register
        Task<Result<AuthResponse>> LoginAsync(AuthRequest request, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

        // Avatar
        Task<Result> UpdateAvatar(string? avatarName, CancellationToken cancellationToken);
        Task<Result> DeleteAvatar(CancellationToken cancellationToken);
        Task<Result<string?>> GetUserAvatarNameAsync(Guid userId, CancellationToken cancellationToken);

        // Role
        Task<Result> AssignRoleAsync(Guid userId, RoleType role, CancellationToken token);

        // Get info
        Task<PagedList<User>> GetUsersAsync(UserQuery query, CancellationToken cancellationToken);
        Task<Result<UserDto>> GetUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<ProfileDto>> GetProfileAsync(CancellationToken cancellationToken, Guid? userId);

        // Ban
        Task<Result> BanUserAsync(Guid userId, string reason, int? durationMinutes, CancellationToken cancellationToken);
        Task<Result> UnbanUserAsync(Guid userId, CancellationToken cancellationToken);

        // Username
        Task<Result> UpdateUsernameAsync(string username, CancellationToken cancellationToken);
    }
}

using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;

namespace Discussly.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> LoginAsync(AuthRequest request, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result> UpdateAvatar(string? avatarName, CancellationToken cancellationToken);
        Task<Result> SoftDeleteAsync(CancellationToken cancellationToken);
        Task<Result> AssignRoleAsync(Guid userId, RoleType role, CancellationToken token);
        Task<PagedList<User>> GetUsersAsync(UserQuery query, CancellationToken cancellationToken);
        Task<Result<UserDto>> GetUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result> BanUserAsync(Guid userId, string reason, int? durationMinutes, CancellationToken cancellationToken);
        Task<Result> UnbanUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<int>> PurgeDeletedUsersAsync(int daysOld, CancellationToken cancellationToken);
    }
}

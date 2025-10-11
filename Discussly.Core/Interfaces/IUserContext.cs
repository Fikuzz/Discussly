
using Discussly.Core.Commons;

namespace Discussly.Core.Interfaces
{
    public interface IUserContext
    {
        string? Email { get; }
        RoleType? Role { get; }
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string? UserName { get; }
    }
}
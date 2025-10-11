using Discussly.Core.Entities;

namespace Discussly.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        (Guid? userId, string? role) ValidateToken(string token);
    }
}

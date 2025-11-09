using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Discussly.Core.Interfaces;
using Discussly.Core.Commons;

namespace Discussly.Application.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(userId, out var guid) ? guid : null;
            }
        }

        public string? UserName => _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.Name)?.Value;

        public string? Email => _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.Email)?.Value;

        public RoleType? Role
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(roleClaim))
                    return null;

                return Enum.TryParse<RoleType>(roleClaim, out var role)
                    ? role
                    : null;
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?
            .User.Identity?.IsAuthenticated ?? false;
    }
}

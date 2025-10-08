using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface IUserService
    {
        public Task<Result<AuthResponse>> LoginAsync(AuthRequest request, CancellationToken token);

        public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken token);
    }
}

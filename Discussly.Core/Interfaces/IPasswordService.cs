using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Password;

namespace Discussly.Core.Interfaces
{
    public interface IPasswordService
    {
        Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
        Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    }
}
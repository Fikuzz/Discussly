using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Password;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IDiscusslyDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IUserContext _userContext;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            IDiscusslyDbContext context,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IUserContext userContext,
            ILogger<PasswordService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim(), cancellationToken);

                if (user == null || user.IsDeleted)
                {
                    return Result.Success();
                }

                var resetToken = PasswordResetToken.Create(user.Id);
                await _context.AddAsync(resetToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Отправляем email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetToken.Token);

                _logger.LogInformation("Password reset token created for user {UserId}", user.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for email: {Email}", email);
                return Result.Success();
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Находим токен
                var token = await _context.PasswordResetTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

                if (token == null || !token.IsValid)
                    return Result.Failure("Invalid or expired reset token");

                if (token.User.IsDeleted)
                    return Result.Failure("Cannot reset password for deleted account");

                // Обновляем пароль
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                token.User.UpdatePasswordHash(newPasswordHash);

                await _context.PasswordResetTokens.Where(t => t == token)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password reset successfully for user {UserId}", token.UserId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password with token: {Token}", request.Token);
                return Result.Failure("Error occurred while resetting password");
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                    return Result.Failure("Not authenticated");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _userContext.UserId, cancellationToken);
                if (user == null)
                    return Result.Failure("User not found");

                if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    return Result.Failure("Current password is incorrect");

                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                user.UpdatePasswordHash(newPasswordHash);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Password changed successfully for user {_userContext.UserId}");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user {_userContext.UserId}");
                return Result.Failure("Error occurred while changing password");
            }
        }
    }
}

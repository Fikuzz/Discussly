using Discussly.Core.Interfaces;
using Microsoft.Extensions.Logging;
namespace Discussly.Application.Services
{
    public class LogEmailService : IEmailService
    {
        private readonly ILogger<LogEmailService> _logger;

        public LogEmailService(ILogger<LogEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string username, string resetToken)
        {
            var resetLink = $"https://http://localhost:8080//reset-password?token={resetToken}";

            _logger.LogInformation($"📧 PASSWORD RESET EMAIL\nTo: {email}\nUsername: {username}\nReset Link: {resetLink}\nToken: {resetToken}");

            await Task.CompletedTask;
        }
    }
}

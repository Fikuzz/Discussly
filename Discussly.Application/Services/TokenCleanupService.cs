using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discussly.Application.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IDiscusslyDbContext>();

                    var deletedCount = await context.PasswordResetTokens
                        .Where(t => t.ExpiresAt < DateTime.UtcNow)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deletedCount > 0)
                        _logger.LogInformation("Cleaned up {Count} expired password reset tokens", deletedCount);

                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Раз в день
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up expired tokens");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}

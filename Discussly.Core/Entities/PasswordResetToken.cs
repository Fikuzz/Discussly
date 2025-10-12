using System.Security.Cryptography;

namespace Discussly.Core.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public string Token { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        private PasswordResetToken() { }

        public static PasswordResetToken Create(Guid userId, int expirationHours = 24)
        {
            return new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = GenerateSecureToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
            };
        }

        public bool IsValid => ExpiresAt > DateTime.UtcNow;

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}

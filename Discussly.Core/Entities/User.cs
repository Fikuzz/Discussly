using Discussly.Core.Commons;
using System.Text.RegularExpressions;

namespace Discussly.Core.Entities
{
    public class User
    {
        public const int USERNAME_MAX_LENGTH = 50;
        public const int USERNAME_MIN_LENGTH = 3;

        public Guid Id { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? AvatarUrl { get; private set; }
        public int Karma { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedAt { get; private set; } = null;
        public RoleType Role { get; private set; } = RoleType.User; 

        public ICollection<Ban> Bans { get; private set; } = new List<Ban>();
        private User() { }

        public static Result<User> Create(string username, string email, string passwordHash, string? avatarUrl = null, RoleType role = RoleType.User)
        {
            var validationResult = ValidateUsername(username)
            .Combine(ValidateEmail(email))
            .Combine(ValidatePasswordHash(passwordHash));

            if (validationResult.IsFailure)
                return Result<User>.Failure(validationResult.Error);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                AvatarUrl = avatarUrl,
                Karma = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null,
                Role = role
            };

            return Result<User>.Success(user);
        }

        private static Result ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Result.Failure("Username cannot be empty or contain only spaces");

            if (username.Length < USERNAME_MIN_LENGTH)
                return Result.Failure($"Username must be at least {USERNAME_MIN_LENGTH} characters");

            if (username.Length > USERNAME_MAX_LENGTH)
                return Result.Failure($"Username must not exceed {USERNAME_MAX_LENGTH} characters");

            return Result.Success();
        }

        private static Result ValidateEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (string.IsNullOrWhiteSpace(email) || !regex.IsMatch(email))
                return Result.Failure("Invalid email format");

            return Result.Success();
        }

        private static Result ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return Result.Failure("Password hash cannot be empty");

            return Result.Success();
        }

        public void UpdateKarma(int change)
        {
            Karma += change;
        }

        public Result UpdateAvatar(string? newAvatarUrl)
        {
            if (!string.IsNullOrWhiteSpace(newAvatarUrl))
            {
                if (!Uri.IsWellFormedUriString(newAvatarUrl, UriKind.Absolute))
                    return Result.Failure("Invalid avatar URL format");

                if (!Uri.TryCreate(newAvatarUrl, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    return Result.Failure("Avatar URL must use HTTP or HTTPS protocol");
                }
            }

            AvatarUrl = String.IsNullOrWhiteSpace(newAvatarUrl) ? null : newAvatarUrl;
            return Result.Success();
        }

        public Result UpdateEmail(string newEmail)
        {
            var validResult = ValidateEmail(newEmail);
            if (validResult.IsFailure)
                return validResult;

            Email = newEmail.Trim().ToLowerInvariant();
            return Result.Success();
        }

        public Result UpdateUsername(string newUsername)
        {
            var validResult = ValidateUsername(newUsername);
            if(validResult.IsFailure)
                return validResult;

            Username = newUsername.Trim();
            return Result.Success();
        }
        public void MarkAsDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }

        public void AssignRole(RoleType role)
        {
            Role = role;
        }
        public void UpdatePasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public bool IsAdmin => Role == RoleType.Admin;
        public bool IsModerator => Role == RoleType.Moderator || IsAdmin;
        public bool IsBanned => GetActiveBan() != null;
        public Ban? GetActiveBan() => Bans.FirstOrDefault(b => b.IsActive);
        public IEnumerable<Ban> GetBanHistory() => Bans.OrderByDescending(b => b.BannedAt);
    }
}
    using Discussly.Core.Commons;

namespace Discussly.Core.Entities
{
    public class Community
    {
        public const int NAME_MAX_LENGTH = 50;
        public const int NAME_MIN_LENGTH = 3;
        public const int DISPLAY_NAME_MAX_LENGTH = 100;
        public const int DISPLAY_NAME_MIN_LENGTH = 3;
        public const int DESCRIPTION_MAX_LENGTH = 500;

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string? AvatarFileName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Guid OwnerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsPublic { get; private set; }

        private Community() { }

        public static Result<Community> Create(string name, string displayName, string? avatarFileName, string description, Guid ownerId, bool isPublic)
        {
            var validateResult = ValidateName(name)
                .Combine(ValidateDisplayName(displayName))
                .Combine(ValidateDescription(description));

            if (validateResult.IsFailure)
                return Result<Community>.Failure(validateResult.Error);

            var community = new Community()
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                DisplayName = displayName.Trim(),
                AvatarFileName = avatarFileName,
                Description = description,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = isPublic
            };

            return Result.Success(community);
        }

        private static Result ValidateName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return Result.Failure("Name cannot be empty or contain only spaces");
            }
            if (name.Length > NAME_MAX_LENGTH)
            {
                return Result.Failure($"Name must be at least {NAME_MIN_LENGTH} characters");
            }
            if (name.Length < NAME_MIN_LENGTH)
            {
                return Result.Failure($"Name must not exceed {NAME_MAX_LENGTH} characters");
            }
            return Result.Success();
        }
        private static Result ValidateDisplayName(string displayName)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                return Result.Failure("Display name cannot be empty or contain only spaces");
            }
            if (displayName.Length > DISPLAY_NAME_MAX_LENGTH)
            {
                return Result.Failure($"Display name must be at least {DISPLAY_NAME_MAX_LENGTH} characters");
            }
            if (displayName.Length < DISPLAY_NAME_MIN_LENGTH)
            {
                return Result.Failure($"Display name must not exceed {DISPLAY_NAME_MIN_LENGTH} characters");
            }
            return Result.Success();
        }
        private static Result ValidateDescription(string description)
        {
            if (description.Length > DESCRIPTION_MAX_LENGTH)
                return Result.Failure($"Discription must be at least {DESCRIPTION_MAX_LENGTH} characters");

            return Result.Success();
        }
        public Result ToPublic()
        {
            IsPublic = true;
            return Result.Success();
        }
        public Result ToPrivate()
        {
            IsPublic = false;
            return Result.Success();
        }
        public Result UpdateDisplayName(string displayName)
        {
            var validateResult = ValidateDisplayName(displayName);
            if (validateResult.IsFailure)
                return validateResult;

            DisplayName = displayName.Trim();

            return Result.Success();
        }
        public Result UpdateAvatar(string?  avatar)
        {
            AvatarFileName = String.IsNullOrWhiteSpace(avatar) ? null : avatar;

            return Result.Success();
        }
        public Result UpdateDescription(string description)
        {
            var validateResult = ValidateDescription(description);
            if (validateResult.IsFailure)
                return validateResult;

            Description = description;

            return Result.Success();
        }
        public Result ChangeOwner(Guid newOwnerId)
        {
            OwnerId = newOwnerId;
            return Result.Success();
        }
    }
}

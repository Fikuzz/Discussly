using Discussly.Core.Commons;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Discussly.Core.Entities
{
    public abstract class MediaAttachment
    {
        public const int MAX_URL_LENGTH = 2000;
        public const int MAX_FILE_SIZE = 100 * 1024 * 1024;

        public Guid Id { get; protected set; }
        public string FileUrl { get; protected set; } = string.Empty;
        public FileType FileType { get; protected set; }
        public string MimeType { get; protected set; } = string.Empty;
        public long FileSize { get; protected set; }
        public string? ThumbnailUrl { get; protected set; }
        public int? Duration { get; protected set; }
        public int SortOrder { get; protected set; }
        [Column(TypeName = "jsonb")]
        public string Metadata { get; protected set; } = string.Empty;

        protected MediaAttachment() { }

        protected static Result ValidateCommon(string fileUrl, long fileSize, int sortOrder)
        {
            return ValidateUrl(fileUrl)
                .Combine(ValidateFileSize(fileSize))
                .Combine(ValidateSortOrder(sortOrder));
        }

        private static Result ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure("File URL cannot be empty");
            if (url.Length > MAX_URL_LENGTH)
                return Result.Failure($"File URL must not exceed {MAX_URL_LENGTH} characters");
            return Result.Success();
        }

        private static Result ValidateFileSize(long fileSize)
        {
            if (fileSize <= 0)
                return Result.Failure("File size must be positive");
            if (fileSize > MAX_FILE_SIZE)
                return Result.Failure($"File size must not exceed {MAX_FILE_SIZE / 1024 / 1024}MB");
            return Result.Success();
        }

        private static Result ValidateSortOrder(int sortOrder)
        {
            if (sortOrder < 0)
                return Result.Failure("Sort order cannot be negative");
            return Result.Success();
        }

        public Result ChangeSortOrder(int newSortOrder)
        {
            var validation = ValidateSortOrder(newSortOrder);
            if (validation.IsFailure) return validation;
            SortOrder = newSortOrder;
            return Result.Success();
        }

        public Result<T> GetMetadata<T>() where T : class
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(Metadata);
                return result != null
                    ? Result<T>.Success(result)
                    : Result<T>.Failure("Failed to deserialize metadata");
            }
            catch (JsonException ex)
            {
                return Result<T>.Failure($"Invalid metadata format: {ex.Message}");
            }
        }
    }
}

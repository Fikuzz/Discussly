using Discussly.Core.Commons;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Discussly.Core.Entities
{
    public abstract class MediaAttachment
    {
        public const int MAX_FILE_SIZE = 100 * 1024 * 1024;

        public Guid Id { get; protected set; }
        public string FileName { get; protected set; } = string.Empty;
        public FileType FileType { get; protected set; }
        public string Path { get; protected set; } = string.Empty;
        public long FileSize { get; protected set; }
        public int SortOrder { get; protected set; }

        [Column(TypeName = "jsonb")]
        public string Metadata { get; protected set; } = string.Empty;

        protected MediaAttachment() { }

        protected static Result ValidateCommon(long fileSize, int sortOrder)
        {
            return ValidateFileSize(fileSize)
                .Combine(ValidateSortOrder(sortOrder));
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

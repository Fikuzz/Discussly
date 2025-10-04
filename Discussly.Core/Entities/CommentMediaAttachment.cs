using Discussly.Core.Common;
using System.Text.Json;

namespace Discussly.Core.Entities
{
    public class CommentMediaAttachment : MediaAttachment
    {
        public Guid CommentId { get; private set; }

        private CommentMediaAttachment() { }

        public static Result<CommentMediaAttachment> Create(
            Guid commentId, string fileUrl, FileType fileType, string mimeType,
            long fileSize, string? thumbnailUrl, int? duration, int sortOrder, object metadata)
        {
            var validateResult = ValidateCommon(fileUrl, fileSize, sortOrder);
            if (validateResult.IsFailure)
                return Result<CommentMediaAttachment>.Failure(validateResult.Error);

            var attachment = new CommentMediaAttachment
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                FileUrl = fileUrl.Trim(),
                FileType = fileType,
                MimeType = mimeType.Trim(),
                FileSize = fileSize,
                ThumbnailUrl = thumbnailUrl?.Trim(),
                Duration = duration >= 0 ? duration : null,
                SortOrder = sortOrder,
                Metadata = JsonSerializer.Serialize(metadata)
            };

            return Result<CommentMediaAttachment>.Success(attachment);
        }
    }
}

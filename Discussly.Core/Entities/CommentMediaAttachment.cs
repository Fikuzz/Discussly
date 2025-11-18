using Discussly.Core.Commons;
using System.Text.Json;

namespace Discussly.Core.Entities
{
    public class CommentMediaAttachment : MediaAttachment
    {
        public Guid CommentId { get; private set; }

        private CommentMediaAttachment() { }

        public static Result<CommentMediaAttachment> Create(
            Guid commentId, FileType fileType, long fileSize, int sortOrder, object metadata)
        {
            var validateResult = ValidateCommon(fileSize, sortOrder);
            if (validateResult.IsFailure)
                return Result<CommentMediaAttachment>.Failure(validateResult.Error);

            var attachment = new CommentMediaAttachment
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                FileType = fileType,
                FileSize = fileSize,
                SortOrder = sortOrder,
                Metadata = JsonSerializer.Serialize(metadata)
            };

            return Result<CommentMediaAttachment>.Success(attachment);
        }
    }
}

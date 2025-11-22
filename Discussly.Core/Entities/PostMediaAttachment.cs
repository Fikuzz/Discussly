using Discussly.Core.Commons;
using System.Text.Json;
namespace Discussly.Core.Entities
{
    public class PostMediaAttachment : MediaAttachment
    {
        public Guid PostId { get; private set; }

        private PostMediaAttachment() { }

        public static Result<PostMediaAttachment> Create(
           Guid id, Guid postId, string FileName, FileType fileType, string path, long fileSize, int sortOrder, object metadata)
        {
            var validateResult = ValidateCommon(fileSize, sortOrder);
            if (validateResult.IsFailure)
                return Result<PostMediaAttachment>.Failure(validateResult.Error);

            var attachment = new PostMediaAttachment
            {
                Id = id,
                FileName = FileName,
                Path = path,
                PostId = postId,
                FileType = fileType,
                FileSize = fileSize,
                SortOrder = sortOrder,
                Metadata = JsonSerializer.Serialize(metadata)
            };

            return Result<PostMediaAttachment>.Success(attachment);
        }
    }
}
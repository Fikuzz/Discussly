using Discussly.Core.Common;
using Discussly.Core.Entities;
using System.Text.Json;

public class PostMediaAttachment : MediaAttachment
{
    public Guid PostId { get; private set; }

    private PostMediaAttachment() { }

    public static Result<PostMediaAttachment> Create(
        Guid postId, string fileUrl, FileType fileType, string mimeType,
        long fileSize, string? thumbnailUrl, int? duration, int sortOrder, object metadata)
    {
        var validateResult = ValidateCommon(fileUrl, fileSize, sortOrder);
        if (validateResult.IsFailure)
            return Result<PostMediaAttachment>.Failure(validateResult.Error);

        var attachment = new PostMediaAttachment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            FileUrl = fileUrl.Trim(),
            FileType = fileType,
            MimeType = mimeType.Trim(),
            FileSize = fileSize,
            ThumbnailUrl = thumbnailUrl?.Trim(),
            Duration = duration >= 0 ? duration : null,
            SortOrder = sortOrder,
            Metadata = JsonSerializer.Serialize(metadata)
        };

        return Result<PostMediaAttachment>.Success(attachment);
    }
}
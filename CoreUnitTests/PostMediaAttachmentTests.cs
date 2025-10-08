using Discussly.Core.Commons;
using Discussly.Core.Entities;

namespace Discussly.Core.UnitTests
{
    public class PostMediaAttachmentTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var fileUrl = "https://cdn.discussly.com/images/photo.jpg";
            var fileType = FileType.Photo;
            var mimeType = "image/jpeg";
            var fileSize = 1024 * 1024; // 1MB
            var metadata = new { width = 1920, height = 1080, format = "JPEG" };

            // Act
            var result = PostMediaAttachment.Create(postId, fileUrl, fileType, mimeType, fileSize, null, null, 1, metadata);

            // Assert
            Assert.True(result.IsSuccess);
            var attachment = result.Value;
            Assert.Equal(postId, attachment.PostId);
            Assert.Equal(fileUrl, attachment.FileUrl);
            Assert.Equal(fileType, attachment.FileType);
            Assert.Equal(mimeType, attachment.MimeType);
            Assert.Equal(fileSize, attachment.FileSize);
            Assert.Equal(1, attachment.SortOrder);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_WithInvalidUrl_ReturnsFailure(string invalidUrl)
        {
            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), invalidUrl, FileType.Photo, "image/jpeg", 1024, null, null, 1, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("cannot be empty", result.Error);
        }

        [Fact]
        public void Create_WithTooLongUrl_ReturnsFailure()
        {
            // Arrange
            var longUrl = "https://" + new string('a', 2000) + ".com/image.jpg";

            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), longUrl, FileType.Photo, "image/jpeg", 1024, null, null, 1, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("must not exceed", result.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithInvalidFileSize_ReturnsFailure(long invalidFileSize)
        {
            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", invalidFileSize, null, null, 1, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("must be positive", result.Error);
        }

        [Fact]
        public void Create_WithTooLargeFileSize_ReturnsFailure()
        {
            // Arrange
            var largeFileSize = 101 * 1024 * 1024; // 101MB > 100MB limit

            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", largeFileSize, null, null, 1, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("must not exceed", result.Error);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Create_WithInvalidSortOrder_ReturnsFailure(int invalidSortOrder)
        {
            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, invalidSortOrder, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("cannot be negative", result.Error);
        }

        [Fact]
        public void Create_WithVideoFile_SetsDuration()
        {
            // Arrange
            var duration = 120; // 2 minutes

            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/video.mp4", FileType.Video, "video/mp4", 1024, null, duration, 1, new object());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(duration, result.Value.Duration);
        }

        [Fact]
        public void Create_WithNegativeDuration_SetsToNull()
        {
            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/video.mp4", FileType.Video, "video/mp4", 1024, null, -1, 1, new object());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Duration);
        }

        [Fact]
        public void Create_WithThumbnailUrl_SetsThumbnail()
        {
            // Arrange
            var thumbnailUrl = "https://cdn.discussly.com/thumbs/thumb.jpg";

            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, thumbnailUrl, null, 1, new object());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(thumbnailUrl, result.Value.ThumbnailUrl);
        }

        [Fact]
        public void Create_WithMetadata_SerializesCorrectly()
        {
            // Arrange
            var metadata = new { width = 1920, height = 1080, format = "JPEG" };

            // Act
            var result = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, metadata);

            // Assert
            Assert.True(result.IsSuccess);
            var deserializedMetadata = result.Value.GetMetadata<dynamic>();
            Assert.NotNull(deserializedMetadata);
        }

        [Fact]
        public void ChangeSortOrder_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var attachment = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, new object()).Value;
            var newSortOrder = 5;

            // Act
            var result = attachment.ChangeSortOrder(newSortOrder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newSortOrder, attachment.SortOrder);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void ChangeSortOrder_WithInvalidOrder_ReturnsFailure(int invalidSortOrder)
        {
            // Arrange
            var attachment = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, new object()).Value;
            var originalSortOrder = attachment.SortOrder;

            // Act
            var result = attachment.ChangeSortOrder(invalidSortOrder);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(originalSortOrder, attachment.SortOrder); // Не изменился
        }

        [Fact]
        public void GetMetadata_WithValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var metadata = new { width = 1920, height = 1080 };
            var attachment = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, metadata).Value;

            // Act
            var result = attachment.GetMetadata<dynamic>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetMetadata_WithInvalidJson_ReturnsFailure()
        {
            // Arrange
            var attachment = PostMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, new object()).Value;

            // Manually corrupt the metadata
            attachment.GetType().GetProperty("Metadata")?.SetValue(attachment, "invalid json");

            // Act
            var result = attachment.GetMetadata<Result<dynamic>>();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Invalid metadata format", result.Error);
        }
    }
}

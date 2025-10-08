using Discussly.Core.Commons;
using Discussly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.UnitTests
{
    public class CommentMediaAttachmentTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var fileUrl = "https://cdn.discussly.com/images/comment-photo.jpg";
            var fileType = FileType.Photo;
            var mimeType = "image/jpeg";
            var fileSize = 512 * 1024; // 512KB
            var metadata = new { width = 800, height = 600 };

            // Act
            var result = CommentMediaAttachment.Create(commentId, fileUrl, fileType, mimeType, fileSize, null, null, 1, metadata);

            // Assert
            Assert.True(result.IsSuccess);
            var attachment = result.Value;
            Assert.Equal(commentId, attachment.CommentId);
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
            var result = CommentMediaAttachment.Create(Guid.NewGuid(), invalidUrl, FileType.Photo, "image/jpeg", 1024, null, null, 1, new object());

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("cannot be empty", result.Error);
        }

        [Fact]
        public void Create_WithAudioFile_SetsDuration()
        {
            // Arrange
            var duration = 180; // 3 minutes

            // Act
            var result = CommentMediaAttachment.Create(Guid.NewGuid(), "https://example.com/audio.mp3", FileType.Audio, "audio/mp3", 1024, null, duration, 1, new object());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(duration, result.Value.Duration);
        }

        [Fact]
        public void Create_WithDifferentFileTypes_ReturnsSuccess()
        {
            // Arrange
            var fileTypes = new[] { FileType.Photo, FileType.Video, FileType.Audio };

            foreach (var fileType in fileTypes)
            {
                // Act
                var result = CommentMediaAttachment.Create(Guid.NewGuid(), "https://example.com/file", fileType, "type", 1024, null, null, 1, new object());

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal(fileType, result.Value.FileType);
            }
        }

        [Fact]
        public void ChangeSortOrder_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var attachment = CommentMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, new object()).Value;
            var newSortOrder = 3;

            // Act
            var result = attachment.ChangeSortOrder(newSortOrder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newSortOrder, attachment.SortOrder);
        }

        [Fact]
        public void GetMetadata_WithComplexObject_ReturnsDeserializedData()
        {
            // Arrange
            var metadata = new
            {
                width = 1920,
                height = 1080,
                format = "JPEG",
                quality = 85,
                hasAlpha = false
            };

            var attachment = CommentMediaAttachment.Create(Guid.NewGuid(), "https://example.com/image.jpg", FileType.Photo, "image/jpeg", 1024, null, null, 1, metadata).Value;

            // Act
            var result = attachment.GetMetadata<dynamic>();

            // Assert
            Assert.NotNull(result);
        }
    }
}

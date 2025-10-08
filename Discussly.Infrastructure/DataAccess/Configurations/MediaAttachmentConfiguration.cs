using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class MediaAttachmentConfiguration : IEntityTypeConfiguration<MediaAttachment>
    {
        public void Configure(EntityTypeBuilder<MediaAttachment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileUrl)
                .HasMaxLength(MediaAttachment.MAX_URL_LENGTH)
                .IsRequired();

            builder.Property(x => x.FileType)
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.MimeType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FileSize)
                .IsRequired();

            builder.Property(x => x.SortOrder);

            builder.HasIndex(x => x.FileType);
        }
    }
}

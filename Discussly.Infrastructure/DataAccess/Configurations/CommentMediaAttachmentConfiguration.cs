using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class CommentMediaAttachmentConfiguration : IEntityTypeConfiguration<CommentMediaAttachment>
    {
        public void Configure(EntityTypeBuilder<CommentMediaAttachment> builder)
        {
            builder.HasOne<Comment>()
                .WithMany(x => x.MediaAttachments)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.CommentId);
        }
    }
}

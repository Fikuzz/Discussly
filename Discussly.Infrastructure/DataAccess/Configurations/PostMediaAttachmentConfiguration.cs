using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class PostMediaAttachmentConfiguration : IEntityTypeConfiguration<PostMediaAttachment>
    {
        public void Configure(EntityTypeBuilder<PostMediaAttachment> builder)
        {
            builder.HasOne<Post>()
                .WithMany(x => x.MediaAttachments)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PostId);
            builder.HasIndex(x => new { x.PostId, x.SortOrder });
        }
    }
}

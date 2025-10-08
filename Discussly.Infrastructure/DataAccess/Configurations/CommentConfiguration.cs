using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ContentText)
                .HasMaxLength(Comment.CONTENT_MAX_LENGTH)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<Post>()
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.PostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Comment>()
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для производительности
            builder.HasIndex(x => x.PostId);
            builder.HasIndex(x => x.ParentCommentId);
        }
    }
}

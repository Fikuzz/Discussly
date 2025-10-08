using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(Post.TITLE_MAX_LENGTH)
                .IsRequired();

            builder.Property(x => x.ContentText)
                .HasMaxLength(Post.CONTENT_MAX_LENGTH);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();

            builder.HasOne<Community>()
                .WithMany()
                .HasForeignKey (p => p.CommunityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Индексы для производительности
            builder.HasIndex(x => x.AuthorId);
            builder.HasIndex(x => x.CommunityId);

            // Составные индексы для частых запросов
            builder.HasIndex(x => new { x.CommunityId, x.CreatedAt });
        }
    }
}

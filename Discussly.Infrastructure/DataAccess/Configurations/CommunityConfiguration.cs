using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class CommunityConfiguration : IEntityTypeConfiguration<Community>
    {
        public void Configure(EntityTypeBuilder<Community> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(Community.NAME_MAX_LENGTH)
                .IsRequired();

            builder.Property(x => x.DisplayName)
                .HasMaxLength(Community.DISPLAY_NAME_MAX_LENGTH)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            // Индексы для производительности
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.IsPublic);
        }
    }
}

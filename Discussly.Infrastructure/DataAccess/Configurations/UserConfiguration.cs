using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username)
                .HasMaxLength(User.USERNAME_MAX_LENGTH)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.AvatarUrl)
                .HasMaxLength(MediaAttachment.MAX_URL_LENGTH);

            // Уникальные индексы
            builder.HasIndex(x => x.Username)
                .IsUnique();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            // Индексы для производительности
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.Karma);
            builder.HasIndex(u => new { u.Email, u.Username });

            builder.HasIndex(x => new { x.IsBanned, x.Karma });
        }
    }
}

using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Discussly.Infrastructure.DataAccess.Configurations
{
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.Token)
                .IsUnique();

            builder.HasIndex(t => t.ExpiresAt);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.ExpiresAt)
                .IsRequired();
        }
    }
}

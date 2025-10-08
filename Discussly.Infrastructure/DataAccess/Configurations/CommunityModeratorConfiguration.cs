using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class CommunityModeratorConfiguration : IEntityTypeConfiguration<CommunityModerator>
    {
        public void Configure(EntityTypeBuilder<CommunityModerator> builder)
        {
            builder.HasKey(x => new { x.UserId, x.CommunityId });

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne<Community>()
                .WithMany()
                .HasForeignKey(x => x.CommunityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CommunityId);
        }
    }
}

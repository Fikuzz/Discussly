using Discussly.Core.Commons;
using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class CommunitySubscriptionConfigration : IEntityTypeConfiguration<CommunitySubscription>
    {
        public void Configure(EntityTypeBuilder<CommunitySubscription> builder)
        {
            builder.HasKey(x => new { x.UserId, x.CommunityId });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Community)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.CommunityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasDefaultValue(CommunityRoleType.User);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CommunityId);
        }
    }
}

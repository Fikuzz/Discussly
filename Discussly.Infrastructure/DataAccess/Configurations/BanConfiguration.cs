using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    public class BanConfiguration : IEntityTypeConfiguration<Ban>
    {
        public void Configure(EntityTypeBuilder<Ban> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(b => b.User)
            .WithMany(u => u.Bans)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Moderator)
                .WithMany()
                .HasForeignKey(b => b.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.Reason).HasMaxLength(500).IsRequired();
        }
    }
}

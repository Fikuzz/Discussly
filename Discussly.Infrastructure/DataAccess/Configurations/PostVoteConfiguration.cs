using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussly.Infrastructure.DataAccess.Configurations
{
    internal class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
    {
        public void Configure(EntityTypeBuilder<PostVote> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne<Post>()
                .WithMany(p => p.Votes)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.VoteType)
                .HasConversion<short>()
                .IsRequired();

            // Уникальный индекс
            builder.HasIndex(x => new { x.UserId, x.PostId })
                .IsUnique();
        }
    }
}

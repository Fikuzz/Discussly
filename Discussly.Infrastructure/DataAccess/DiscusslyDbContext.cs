using Discussly.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Discussly.Infrastructure.DataAccess
{
    public class DiscusslyDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Community> Communities => Set<Community>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostVote> PostVotes => Set<PostVote>();
        public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
        public DbSet<PostMediaAttachment> PostMediaAttachments => Set<PostMediaAttachment>();
        public DbSet<CommentMediaAttachment> CommentMediaAttachments => Set<CommentMediaAttachment>();
        public DbSet<CommunityModerator> CommunityModerators => Set<CommunityModerator>();
        public DbSet<CommunitySubscription> CommunitySubscriptions => Set<CommunitySubscription>();

        public DiscusslyDbContext(DbContextOptions<DiscusslyDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscusslyDbContext).Assembly);
        }
    }
}

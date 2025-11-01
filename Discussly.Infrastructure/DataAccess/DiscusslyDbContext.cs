using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Discussly.Infrastructure.DataAccess
{
    public class DiscusslyDbContext : DbContext, IDiscusslyDbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Community> Communities => Set<Community>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostVote> PostVotes => Set<PostVote>();
        public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
        public DbSet<PostMediaAttachment> PostMediaAttachments => Set<PostMediaAttachment>();
        public DbSet<CommentMediaAttachment> CommentMediaAttachments => Set<CommentMediaAttachment>();
        public DbSet<CommunitySubscription> CommunitySubscriptions => Set<CommunitySubscription>();
        public DbSet<Ban> Bans => Set<Ban>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        public DiscusslyDbContext(DbContextOptions<DiscusslyDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscusslyDbContext).Assembly);
        }

        public void Add<T>(T entity) where T : class => base.Add(entity);

        public async Task AddAsync<T>(T entity) where T : class
            => await base.AddAsync(entity);

        public void AddRange<T>(IEnumerable<T> entities) where T : class
            => base.AddRange(entities);

        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
            => await base.AddRangeAsync(entities);

        public void Remove<T>(T entity) where T : class
            => base.Remove(entity);

        public void RemoveRange<T>(IEnumerable<T> entities) where T : class
            => base.RemoveRange(entities);

        IQueryable<User> IDiscusslyDbContext.Users => Users;
        IQueryable<Community> IDiscusslyDbContext.Communities => Communities;
        IQueryable<Post> IDiscusslyDbContext.Posts => Posts;
        IQueryable<Comment> IDiscusslyDbContext.Comments => Comments;
        IQueryable<PostVote> IDiscusslyDbContext.PostVotes => PostVotes;
        IQueryable<CommentVote> IDiscusslyDbContext.CommentVotes => CommentVotes;
        IQueryable<PostMediaAttachment> IDiscusslyDbContext.PostMediaAttachments => PostMediaAttachments;
        IQueryable<CommentMediaAttachment> IDiscusslyDbContext.CommentMediaAttachments => CommentMediaAttachments;
        IQueryable<CommunitySubscription> IDiscusslyDbContext.CommunitySubscriptions => CommunitySubscriptions;
        IQueryable<Ban> IDiscusslyDbContext.Bans => Bans;
        IQueryable<PasswordResetToken> IDiscusslyDbContext.PasswordResetTokens => PasswordResetTokens;
    }
}

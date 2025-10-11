using Discussly.Core.Entities;

namespace Discussly.Core.Interfaces
{
    public interface IDiscusslyDbContext
    {
        IQueryable<User> Users { get; }
        IQueryable<Community> Communities { get; }
        IQueryable<Post> Posts { get; }
        IQueryable<Comment> Comments { get; }
        IQueryable<PostVote> PostVotes { get; }
        IQueryable<CommentVote> CommentVotes { get; }
        IQueryable<PostMediaAttachment> PostMediaAttachments { get; }
        IQueryable<CommentMediaAttachment> CommentMediaAttachments { get; }
        IQueryable<CommunityModerator> CommunityModerators { get; }
        IQueryable<CommunitySubscription> CommunitySubscriptions { get; }
        IQueryable<Ban> Bans { get; }

        void Add<T>(T entity) where T : class;
        Task AddAsync<T>(T entity) where T : class;
        void AddRange<T>(IEnumerable<T> entities) where T : class;
        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class;
        void Remove<T>(T entity) where T : class;
        void RemoveRange<T>(IEnumerable<T> entities) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

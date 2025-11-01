using Discussly.Core.Commons;

namespace Discussly.Core.Entities
{
    public class CommunitySubscription
    {
        public Guid UserId { get; private set; }
        public Guid CommunityId { get; private set; }
        public DateTime SubscribedAt { get; private set; }
        public CommunityRoleType Role { get; private set; }
        public virtual User User { get; private set; }
        public virtual Community Community { get; private set; }
        private CommunitySubscription() { }

        public static Result<CommunitySubscription> Create(Guid userId, Guid communityId)
        {
            var communitySubscription = new CommunitySubscription()
            {
                UserId = userId,
                CommunityId = communityId,
                SubscribedAt = DateTime.UtcNow,
                Role = CommunityRoleType.User
            };

            return Result<CommunitySubscription>.Success(communitySubscription);
        }
    }
}

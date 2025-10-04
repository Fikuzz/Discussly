using Discussly.Core.Common;

namespace Discussly.Core.Entities
{
    public class CommunitySubscription
    {
        public Guid UserId { get; private set; }
        public Guid CommunityId { get; private set; }
        public DateTime SubscribedAt { get; private set; }

        private CommunitySubscription() { }

        public static Result<CommunitySubscription> Create(Guid userId, Guid communityId)
        {
            var communitySubscription = new CommunitySubscription()
            {
                UserId = userId,
                CommunityId = communityId,
                SubscribedAt = DateTime.UtcNow
            };

            return Result<CommunitySubscription>.Success(communitySubscription);
        }
    }
}

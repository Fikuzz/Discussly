using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface ICommunitySubscriptionService
    {
        Task<Result> SubscribeAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result> UnsubscribeAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<bool>> CheckSubscriptionAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommunityDto>>> UserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<ICollection<MemberDto>>> CommunitySubsribtionsAsync(Guid communityId, CancellationToken cancellationToken);
    }
}

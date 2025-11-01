using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface ICommunitySubscriptionService
    {
        Task<Result<MemberDto>> SubscribeAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result> UnsubscribeAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<MemberDto?>> CheckSubscriptionAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommunityDto>>> UserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<ICollection<MemberDto>>> CommunitySubsribtionsAsync(Guid communityId, CancellationToken cancellationToken);
    }
}

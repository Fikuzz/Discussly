using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface ICommunitySubscriptionService
    {
        Task<Result> Subscribe(Guid communityId, CancellationToken cancellationToken);
        Task<Result> Unsubscribe(Guid communityId, CancellationToken cancellationToken);
        Task<Result<bool>> CheckSubscription(Guid communityId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommunityDto>>> UserSubscriptions(Guid userId, CancellationToken cancellationToken);
        Task<Result<ICollection<UserDto>>> CommunitySubsribtions(Guid communityId, CancellationToken cancellationToken);
    }
}

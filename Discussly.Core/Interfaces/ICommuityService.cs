using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface ICommuityService
    {
        Task<Result<CommunityDto>> GetAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommunityDto>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<Guid>> CreateAsync(CommunityCreateRequest createRequest, CancellationToken cancellationToken);
    }
}

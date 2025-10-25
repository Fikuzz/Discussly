using Discussly.Core.Commons;
using Discussly.Core.DTOs;

namespace Discussly.Core.Interfaces
{
    public interface ICommuityService
    {
        Task<Result<CommunityDTO>> GetAsync(Guid communityId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommunityDTO>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<Guid>> CreateAsync(CommunityCreateRequest createRequest, CancellationToken cancellationToken);
    }
}

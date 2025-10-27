using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;

namespace Discussly.Core.Interfaces
{
    public interface IPostService
    {
        Task<Result<Guid>> CreateAsync(CreatePostDto dto, CancellationToken cancellationToken);
        Task<Result<ICollection<PostDto>>> GetAll(CancellationToken cancellationToken);
        Task<Result<ICollection<PostDto>>> GetCommunityPost(Guid communityId, CancellationToken cancellationToken);
        Task<Result<PostDto>> GetById(Guid id, CancellationToken cancellationToken);
    }
}

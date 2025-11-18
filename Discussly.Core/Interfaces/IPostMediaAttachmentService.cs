using Discussly.Core.Commons;
using Microsoft.AspNetCore.Http;

namespace Discussly.Core.Interfaces
{
    public interface IPostMediaAttachmentService
    {
        Task<Result> AddAsync(Guid postId, IFormFile file, CancellationToken cancellationToken);
    }
}

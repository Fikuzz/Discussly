using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;

namespace Discussly.Core.Interfaces
{
    public interface ICommentService
    {
        Task<Result<Guid>> AddAsync(CreateCommentDto dto, CancellationToken cancellationToken);
        Task<Result<ICollection<CommentDto>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<ICollection<CommentDto>>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken);
        Task<Result<ICollection<CommentDto>>> GetSubCommentAsync(Guid commentId, CancellationToken cancellationToken);
        Task<Result> Delete(Guid id,  CancellationToken cancellationToken);
    }
}

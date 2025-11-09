using Discussly.Application.Interfaces;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ICommentVoteService _commentVoteService;
        private readonly IPostService _postService;
        private readonly IStorageService _storageService;

        public CommentController(ICommentService commentService, IPostService postService, IStorageService storageService, ICommentVoteService commentVoteService)
        {
            _commentService = commentService;
            _postService = postService;
            _storageService = storageService;
            _commentVoteService = commentVoteService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateComment(CreateCommentDto dto, CancellationToken cancellationToken)
        {
            var result = await _commentService.AddAsync(dto, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<CommentDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _commentService.GetAllAsync(cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}/subcomments")]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<CommentDto>>> GetSubComment(Guid id, CancellationToken cancellationToken)
        {
            var result = await _commentService.GetSubCommentAsync(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}/delete")]
        public async Task<ActionResult> DeleteComment(Guid id, CancellationToken cancellationToken)
        {
            var result = await _commentService.Delete(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpGet("{id:guid}/vote")]
        public async Task<ActionResult<VoteType>> GetVote(Guid id, CancellationToken cancellationToken)
        {
            var result = await _commentVoteService.GetVoteType(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{id:guid}/vote")]
        public async Task<ActionResult> SetVote(Guid id, VoteType voteType, CancellationToken cancellationToken)
        {
            var result = await _commentVoteService.VoteAsync(id, voteType, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}

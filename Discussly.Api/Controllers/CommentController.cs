using Discussly.Application.Interfaces;
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
        private readonly IPostService _postService;
        private readonly IStorageService _storageService;

        public CommentController(ICommentService commentService, IPostService postService, IStorageService storageService)
        {
            _commentService = commentService;
            _postService = postService;
            _storageService = storageService;
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
    }
}

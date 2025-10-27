using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Post;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public PostController(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<PostDto>>> GetAllPosts(CancellationToken cancellationToken)
        {
            var posts = await _postService.GetAll(cancellationToken);

            if (posts.IsFailure)
                return BadRequest(posts.Error);

            return Ok(posts.Value);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreatePosts(CreatePostDto dto, CancellationToken cancellationToken)
        {
            var result = await _postService.CreateAsync(dto, cancellationToken);

            if(result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PostDto>> GetPost(Guid id, CancellationToken cancellationToken)
        {
            var result = await _postService.GetById(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{id:guid}/comments")]
        public async Task<ActionResult<ICollection<CommentDto>>> GetPostComment(Guid id, CancellationToken cancellationToken)
        {
            var result = await _commentService.GetPostCommentsAsync(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}

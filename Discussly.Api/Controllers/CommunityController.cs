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
    public class CommunityController : ControllerBase
    {
        private readonly ICommuityService _commuityService;
        private readonly IPostService _postService;
        private readonly IStorageService _storageService;
        private readonly ICommunitySubscriptionService _communitySubscriptionService;

        public CommunityController(ICommuityService commuityService, IStorageService storageService, IPostService postService, ICommunitySubscriptionService communitySubscriptionService)
        {
            _commuityService = commuityService;
            _storageService = storageService;
            _postService = postService;
            _communitySubscriptionService = communitySubscriptionService;
        }

        [HttpGet("{communityId:Guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<CommunityDto>> GetCommunity(Guid communityId, CancellationToken cancellationToken)
        {
            var result = await _commuityService.GetAsync(communityId, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<CommunityDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _commuityService.GetAllAsync(cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> CreateCommunity(CommunityCreateRequest communityCreateRequest, CancellationToken cancellationToken)
        {
            var result = await _commuityService.CreateAsync(communityCreateRequest, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{communityId:Guid}/posts")]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<PostDto>>> GetCommunityPosts(Guid communityId, CancellationToken cancellationToken)
        {
            var posts = await _postService.GetCommunityPost(communityId, cancellationToken);

            if (posts.IsFailure)
                return BadRequest(posts.Error);

            return Ok(posts.Value);
        }

        [HttpGet("{communityId:Guid}/subscriptions")]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<MemberDto?>>> GetSubscriptions(Guid communityId, CancellationToken cancellationToken)
        {
            var result = await _communitySubscriptionService.CommunitySubsribtionsAsync(communityId, cancellationToken);
             
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{communityId:Guid}/checkSubscription")]
        public async Task<ActionResult<MemberDto>> CheckSubscription(Guid communityId, CancellationToken cancellationToken)
        {
            var result = await _communitySubscriptionService.CheckSubscriptionAsync(communityId, cancellationToken);

            if (result.IsFailure) 
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{communityId:guid}/subscribe")]
        public async Task<ActionResult<MemberDto>> Subscribe(Guid communityId, CancellationToken cancellationToken)
        {
            var result = await _communitySubscriptionService.SubscribeAsync(communityId, cancellationToken);

            if(result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("{communityId:guid}/unsubscribe")]
        public async Task<ActionResult> Unsubscribe(Guid communityId, CancellationToken cancellationToken)
        {
            var result = await _communitySubscriptionService.UnsubscribeAsync(communityId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}

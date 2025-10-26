using Discussly.Application.Interfaces;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Services;

namespace Discussly.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        IUserService _userService;
        private readonly IUserContext _userContext;
        private readonly IStorageService _storageService;

        public UserController(IUserService userService, IUserContext userContext, IStorageService storageService)
        {
            _userService = userService;
            _userContext = userContext;
            _storageService = storageService;
        }

        //Deleting
        [HttpDelete("delete-my-account")]
        public async Task<ActionResult> DeleteMyAccount(CancellationToken cancellationToken)
        {
            var result = await _userService.SoftDeleteAsync(cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("purge-deleted-users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PurgeDeletedUsers(CancellationToken cancellationToken, [FromQuery] int daysOld = 30)
        {
            var result = await _userService.PurgeDeletedUsersAsync(daysOld, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok($"Was deleted {result.Value} user(-s)");
        }

        [HttpPost("restore/{userId:guid}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult> RestoreUser(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.RestoreAsync(userId, cancellationToken);

            if (result.IsFailure) 
                return BadRequest(result.Error);

            return Ok();
        }

        //Role
        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
                return Unauthorized();

            if (!Enum.TryParse(request.Role, out RoleType role))
                return BadRequest("Incorrect role name");

            var result = await _userService.AssignRoleAsync(
                request.TargetUserId,
                role,
                cancellationToken
            );

            if (result.IsFailure) 
                return BadRequest(result.Error);

            return Ok(new { message = $"Role {request.Role} assigned to user {request.TargetUserId}" });
        }

        //Get
        [HttpGet]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult> GetUsers([FromQuery] UserQuery query, CancellationToken cancellationToken)
        {
            var users = await _userService.GetUsersAsync(query, cancellationToken);

            var userDtos = new PagedList<UserDto>(
                users.Items.Select(UserDto.Map).ToList(),
                users.TotalCount,
                users.Page,
                users.PageSize
                );

            return Ok(userDtos);
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetUserAsync(userId, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileDto>> GetMyProfile(CancellationToken cancellationToken)
        {
            if (!_userContext.IsAuthenticated)
                return Unauthorized();

            var result = await _userService.GetProfileAsync(cancellationToken, userId: null);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("profile/{userId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProfileDto>> GetUserProfile(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetProfileAsync(cancellationToken, userId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        //Ban
        [HttpPost("{userId:guid}/ban")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult> BanUser(Guid userId, [FromBody] BanRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.BanUserAsync(userId, request.Reason, request.DurationMinutes, cancellationToken);
            if(result.IsFailure) 
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("{userId:guid}/unban")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult> UnbanUser(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.UnbanUserAsync(userId, cancellationToken);
            if( result.IsFailure) 
                return BadRequest( result.Error);

            return Ok();
        }

        
        //Avatar
        [HttpPut("avatar")]
        public async Task<ActionResult> UpdateAvatar(IFormFile formFile, CancellationToken cancellationToken)
        {
            if (_userContext.UserId == null)
                return BadRequest("Couldn't get User Id");

            var oldAvatarName = await _userService.GetUserAvatarNameAsync(_userContext.UserId.Value, cancellationToken);
            if (oldAvatarName.IsSuccess && oldAvatarName.Value != null)
            {
                _storageService.DeleteMedia(oldAvatarName.Value, Storage.UserAvatar);
            }

            var storageResult = await _storageService.SaveMediaAsync(_userContext.UserId.Value, Storage.UserAvatar, formFile);

            if(storageResult.IsFailure)
                return BadRequest(storageResult.Error);

            var result = await _userService.UpdateAvatar(storageResult.Value, cancellationToken);

            return Ok(storageResult.Value);
        }

        [HttpDelete("avatar")]
        public async Task<ActionResult> DeleteAvatar(CancellationToken cancellationToken)
        {
            if (_userContext.UserId == null)
                return BadRequest("Couldn't get User Id");

            var oldAvatarName = await _userService.GetUserAvatarNameAsync(_userContext.UserId.Value, cancellationToken);
            if (oldAvatarName.IsSuccess && oldAvatarName.Value != null)
            {
                _storageService.DeleteMedia(oldAvatarName.Value, Storage.UserAvatar);
            }

            var result = await _userService.UpdateAvatar(null, cancellationToken);

            if(result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }


        //Username
        [HttpPut("username")]
        public async Task<ActionResult> UpdateUsername(string username, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateUsernameAsync(username, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        //Karma
        [HttpPost("karma/increment")]
        public async Task<ActionResult> IncrementKarma(CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateKarmaAsync(1, cancellationToken);

            if(result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("karma/decrement")]
        public async Task<ActionResult> DecrementKarma(CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateKarmaAsync(-1, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

    }
}
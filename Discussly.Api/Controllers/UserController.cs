using Discussly.Application.Interfaces;
using Discussly.Core.Commons;
using Discussly.Core.DTOs;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        [HttpDelete("delete-my-account")]
        public async Task<ActionResult> DeleteMyAccount(
            CancellationToken cancellationToken)
        {
            var result = await _userService.SoftDeleteAsync(cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("restore/{userId:guid}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult> RestoreUser(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var result = await _userService.RestoreAsync(userId, cancellationToken);

            if (result.IsFailure) 
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRole(
            [FromBody] AssignRoleRequest request,
            CancellationToken cancellationToken)
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

        // GET /api/users - список пользователей с пагинацией
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

        // GET /api/users/{userId} - детали пользователя
        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult<UserDto>> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetUserAsync(userId, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result);
        }

        // POST /api/users/{userId}/ban - забанить пользователя
        [HttpPost("{userId:guid}/ban")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult> BanUser(Guid userId, [FromBody] BanRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.BanUserAsync(userId, request.Reason, request.DurationMinutes, cancellationToken);
            if(result.IsFailure) 
                return BadRequest(result.Error);

            return Ok();
        }

        // POST /api/users/{userId}/unban - разбанить пользователя 
        [HttpPost("{userId:guid}/unban")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<ActionResult> UnbanUser(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.UnbanUserAsync(userId, cancellationToken);
            if( result.IsFailure) 
                return BadRequest( result.Error);

            return Ok();
        }

        // POST /api/admin/purge-deleted-users - полное удаление старых аккаунтов 
        [HttpPost("purge-deleted-users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PurgeDeletedUsers(CancellationToken cancellationToken, [FromQuery] int daysOld = 30)
        {
            var result = await _userService.PurgeDeletedUsersAsync(daysOld, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok($"Was deleted {result.Value} user(-s)");
        }

        // PUT /api/user/avatar - обновить аватар 
        [HttpPut("avatar")]
        public async Task<ActionResult> UpdateAvatar(IFormFile formFile, CancellationToken cancellationToken)
        {
            if (_userContext.UserId == null)
                return BadRequest("Couldn't get User Id");

            var oldAvatarName = await _userService.GetUserAvatarNameAsync(_userContext.UserId.Value, cancellationToken);
            if (oldAvatarName.IsSuccess && oldAvatarName.Value != null)
            {
                _storageService.DeleteAvatar(oldAvatarName.Value);
            }

            var storageResult = await _storageService.SaveAvatarAsync(_userContext.UserId.Value, formFile);

            if(storageResult.IsFailure)
                return BadRequest(storageResult.Error);

            var result = await _userService.UpdateAvatar(storageResult.Value, cancellationToken);

            return Ok(storageResult.Value);
        }

        [HttpPut("username")]
        public async Task<ActionResult> UpdateUsername(string username, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateUsernameAsync(username, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
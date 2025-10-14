using Discussly.Application.Services;
using Discussly.Core.DTOs;
using Discussly.Core.DTOs.Password;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;

        public AuthController(IUserService userService, IPasswordService passwordService)
        {
            _userService = userService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
        {
            var result = await _userService.RegisterAsync(request, cancellationToken);

            return result.Match<ActionResult<AuthResponse>>(
                onSuccess: authResponse => Ok(authResponse),
                onFailure: error => BadRequest(new { error })
            );
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(
            AuthRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.LoginAsync(request, cancellationToken);

            return result.Match<ActionResult<AuthResponse>>(
                onSuccess: authResponse => Ok(authResponse),
                onFailure: error => Unauthorized(new { error })
            );
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _passwordService.ForgotPasswordAsync(request.Email, cancellationToken);

            return Ok(new { message = "Reset instructions have been sent" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _passwordService.ResetPasswordAsync(request, cancellationToken);

            if(result.IsFailure)
                return BadRequest(result.Error);

            return Ok("Password reset successfully");
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _passwordService.ChangePasswordAsync(request, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok("Password change successfully");
        }
    }
}

namespace Discussly.Core.DTOs
{
    public record RegisterRequest
    (
        string Username,
        string Email,
        string Password,
        string? AvatarUrl = null
    );
}

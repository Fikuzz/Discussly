namespace Discussly.Core.DTOs
{
    public record AuthResponse
    (
        UserDto User,
        string Token
    );
}

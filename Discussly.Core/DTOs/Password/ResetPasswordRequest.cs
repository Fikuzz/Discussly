namespace Discussly.Core.DTOs
{
    public record ResetPasswordRequest(
    string Token,
    string NewPassword
    );
}

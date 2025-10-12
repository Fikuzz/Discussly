namespace Discussly.Core.DTOs.Password
{
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}

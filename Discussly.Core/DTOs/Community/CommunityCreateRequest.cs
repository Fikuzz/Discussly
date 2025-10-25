namespace Discussly.Core.DTOs
{
    public record CommunityCreateRequest(
        string Name,
        string Description,
        string? AvatarFileName,
        bool IsPublic
    );
}

namespace Discussly.Core.DTOs
{
    public record CommunityCreateRequest(
        string Name,
        string Description,
        bool IsPublic
    );
}

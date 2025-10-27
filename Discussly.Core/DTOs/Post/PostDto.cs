namespace Discussly.Core.DTOs
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentText { get; set; } = string.Empty;
        public UserDto? Author { get; set; } = null!;
        public CommunityDto? Community { get; set; } = null!;
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MediaPreviewFileName { get; set; }
    }
}

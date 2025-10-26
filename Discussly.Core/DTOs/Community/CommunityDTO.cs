using Discussly.Core.Entities;

namespace Discussly.Core.DTOs
{
    public class CommunityDTO
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public int ParticipantCount { get; set; }
        public int PostCount { get; set; }
    }
}

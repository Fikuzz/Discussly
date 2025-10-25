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

        public static CommunityDTO Map(Community community)
        {
            return new CommunityDTO()
            {
                Id = community.Id,
                DisplayName = community.DisplayName,
                Description = community.Description,
                AvatarFileName = community.AvatarFileName,
                CreatedAt = community.CreatedAt,
            };
        }

        public static ICollection<CommunityDTO> MapList(IEnumerable<Community> communities)
        {
            return communities.Select(Map).ToList();
        }
    }
}

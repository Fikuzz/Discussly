using Microsoft.AspNetCore.Http;

namespace Discussly.Core.DTOs.Post
{
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string ContentText { get; set; } = string.Empty;
        public Guid CommunityId { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
    }
}

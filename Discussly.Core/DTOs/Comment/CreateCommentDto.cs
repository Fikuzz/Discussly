using Microsoft.AspNetCore.Http;

namespace Discussly.Core.DTOs
{
    public class CreateCommentDto
    {
        public string Text { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Guid? CommentId { get; set; }
        public IFormFile? Media {  get; set; }
    }
}

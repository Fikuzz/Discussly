namespace Discussly.Core.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public UserDto Author { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool IsEditing { get; set; } = false;
    }
}

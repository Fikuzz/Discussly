namespace Discussly.Core.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public UserDto Author { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public int Score { get; set; }
        public bool IsEditing { get; set; } = false;
    }
}

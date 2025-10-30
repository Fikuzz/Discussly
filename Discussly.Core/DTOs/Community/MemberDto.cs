namespace Discussly.Core.DTOs
{
    public class MemberDto
    {
        public UserDto User { get; set; } = new UserDto();
        public DateTime MemberAt { get; set; }
    }
}

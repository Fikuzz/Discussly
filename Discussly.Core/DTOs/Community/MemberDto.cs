using Discussly.Core.Commons;

namespace Discussly.Core.DTOs
{
    public class MemberDto
    {
        public UserDto User { get; set; } = new UserDto();
        public CommunityRoleType Role { get; set; }
        public DateTime MemberAt { get; set; }
    }
}

using Discussly.Core.Entities;

namespace Discussly.Core.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; }

        public static UserDto Map(User user)
        {
            var dto = new UserDto()
            {
                Id = user.Id,
                Username = user.Username,
                AvatarFileName = user.AvatarFileName
            };

            return dto;
        }

        public static List<UserDto> MapList(IEnumerable<User> users)
        {
            return users.Select(Map).ToList();
        }
    }
}

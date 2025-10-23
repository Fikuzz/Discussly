using Discussly.Core.Commons;
using Discussly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.DTOs
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; }
        public int Karma { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

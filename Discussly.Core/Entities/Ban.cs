using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.Entities
{
    public class Ban
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public Guid ModeratorId { get; private set; }
        public User Moderator { get; private set; } = null!;

        public string Reason { get; private set; } = string.Empty;
        public DateTime BannedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public DateTime? UnbannedAt { get; private set; }
        public Guid? UnbannedByModeratorId { get; private set; }

        public bool IsActive => ExpiresAt == null || ExpiresAt > DateTime.UtcNow;
        public bool IsPermanent => ExpiresAt == null;
        public bool IsTemporary => ExpiresAt.HasValue;

        private Ban() { }

        public static Ban Create(Guid userId, Guid moderatorId, string reason, DateTime? expiresAt = null)
        {
            return new Ban
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ModeratorId = moderatorId,
                Reason = reason,
                BannedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };
        }

        public void Unban(Guid moderatorId)
        {
            UnbannedAt = DateTime.UtcNow;
            UnbannedByModeratorId = moderatorId;
        }
    }
}
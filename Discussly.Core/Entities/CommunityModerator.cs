using Discussly.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.Entities
{
    public class CommunityModerator
    {
        public Guid UserId { get; private set; }
        public Guid CommunityId { get; private set; }
        public DateTime AssignedAt { get; private set; }

        public static Result<CommunityModerator> Create(Guid userId, Guid communityId)
        {
            var moderator = new CommunityModerator
            {
                UserId = userId,
                CommunityId = communityId,
                AssignedAt = DateTime.UtcNow
            };
            return Result<CommunityModerator>.Success(moderator);
        }
    }
}

using Discussly.Core.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.Interfaces
{
    public interface IPostVoteService
    {
        Task<Result> VoteAsync(Guid id, VoteType voteType, CancellationToken cancellationToken);

        Task<Result<VoteType>> GetVoteType(Guid id, CancellationToken cancellationToken);
    }
}

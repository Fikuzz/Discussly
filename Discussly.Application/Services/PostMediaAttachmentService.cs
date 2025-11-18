using Discussly.Core.Commons;
using Discussly.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Application.Services
{
    public class PostMediaAttachmentService : IPostMediaAttachmentService
    {
        private readonly IDiscusslyDbContext _context;

        public PostMediaAttachmentService(IDiscusslyDbContext context)
        {
            _context = context;
        }

        public Task<Result> AddAsync(Guid postId, IFormFile file, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

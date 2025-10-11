using Discussly.Core.Commons;
using Discussly.Core.Entities;
using Discussly.Core.Interfaces;

namespace Discussly.Infrastructure.DataAccess
{
    public class SeedData
    {
        public static void Initialize(DiscusslyDbContext context, IPasswordHasher passwordHasher)
        {
            if (!context.Users.Any(u => u.Role == RoleType.Admin))
            {
                var adminPasswordHash = passwordHasher.HashPassword("root");

                var admin = User.Create(
                    "admin",
                    "admin@discussly.com",
                    adminPasswordHash,
                    null,
                    RoleType.Admin
                ).Value;

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}

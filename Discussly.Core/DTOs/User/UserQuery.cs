using Discussly.Core.Commons;
using Discussly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.DTOs
{
    public record UserQuery
    {
        // Пагинация
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;

        // Фильтрация
        public string? Search { get; init; }
        public RoleType? Role { get; init; }
        public bool? IsDeleted { get; init; }
        public DateTime? CreatedAfter { get; init; }
        public DateTime? CreatedBefore { get; init; }

        // Фильтры по бану
        public bool? HasActiveBan { get; init; }
        public string? BanReasonContains { get; init; }
        public DateTime? BannedAfter { get; init; }
        public DateTime? BannedBefore { get; init; }

        // Сортировка
        public string SortBy { get; init; } = "CreatedAt";
        public bool SortDescending { get; init; } = true;

        // Вычисляемые свойства
        public int Skip => (Page - 1) * PageSize;
        public int Take => PageSize;

        public IQueryable<User> ApplyBanFilters(IQueryable<User> query)
        {
            if (HasActiveBan.HasValue)
            {
                query = HasActiveBan.Value
                    ? query.Where(u => u.Bans.Any(b =>
                        b.UnbannedAt == null &&
                        (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)))
                    : query.Where(u => !u.Bans.Any(b =>
                        b.UnbannedAt == null &&
                        (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)));
            }

            if (!string.IsNullOrEmpty(BanReasonContains))
            {
                query = query.Where(u => u.Bans.Any(b =>
                    b.Reason.Contains(BanReasonContains) &&
                    b.UnbannedAt == null &&
                    (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)));
            }

            if (BannedAfter.HasValue)
            {
                query = query.Where(u => u.Bans.Any(b =>
                    b.BannedAt >= BannedAfter.Value &&
                    b.UnbannedAt == null &&
                    (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)));
            }

            if (BannedBefore.HasValue)
            {
                query = query.Where(u => u.Bans.Any(b =>
                    b.BannedAt <= BannedBefore.Value &&
                    b.UnbannedAt == null &&
                    (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)));
            }

            return query;
        }
    }
}

using System.Text;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Admin.Queries.ExportUsers;

public class ExportUsersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportUsersQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportUsersQuery request, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.IsEmailVerified,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Email,FirstName,LastName,Phone,EmailVerified,Active,LastLogin,CreatedAt");

        foreach (var u in users)
        {
            sb.AppendLine(string.Join(',',
                u.Id,
                Escape(u.Email),
                Escape(u.FirstName),
                Escape(u.LastName),
                Escape(u.PhoneNumber ?? ""),
                u.IsEmailVerified,
                u.IsActive,
                u.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string s) =>
        s.Contains(',') || s.Contains('"') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
}

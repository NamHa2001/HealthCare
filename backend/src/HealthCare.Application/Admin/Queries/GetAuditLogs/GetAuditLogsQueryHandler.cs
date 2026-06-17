using HealthCare.Application.Admin.DTOs;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Admin.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Resource))
            query = query.Where(a => a.Resource == request.Resource);

        if (!string.IsNullOrWhiteSpace(request.EventType))
            query = query.Where(a => a.EventType == request.EventType);

        var total = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        // Batch-load user emails to avoid N+1
        var userIds = logs
            .Where(l => l.UserId.HasValue)
            .Select(l => l.UserId!.Value)
            .Distinct()
            .ToList();

        var emails = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, ct);

        var dtos = logs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            EventType = a.EventType,
            UserId = a.UserId,
            UserEmail = a.UserId.HasValue && emails.TryGetValue(a.UserId.Value, out var email) ? email : null,
            Resource = a.Resource,
            Action = a.Action,
            EntityId = a.EntityId,
            IpAddress = a.IpAddress,
            CreatedAt = a.CreatedAt,
        }).ToList();

        return new PagedResult<AuditLogDto>(dtos, total, request.Page, request.PageSize);
    }
}

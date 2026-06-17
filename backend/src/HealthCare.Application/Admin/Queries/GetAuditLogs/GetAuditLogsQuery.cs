using HealthCare.Application.Admin.DTOs;
using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Admin.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    string? Resource = null,
    string? EventType = null,
    int Page = 1,
    int PageSize = 30
) : IRequest<PagedResult<AuditLogDto>>;

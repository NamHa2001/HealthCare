using MediatR;

namespace HealthCare.Application.Admin.Queries.ExportUsers;

/// <summary>SRS §10.1 — Export danh sách user ra CSV.</summary>
public record ExportUsersQuery : IRequest<byte[]>;

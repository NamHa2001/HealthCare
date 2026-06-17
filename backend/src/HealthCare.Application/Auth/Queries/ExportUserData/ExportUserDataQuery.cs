using MediatR;

namespace HealthCare.Application.Auth.Queries.ExportUserData;

/// <summary>SRS §9.5 — Export toàn bộ dữ liệu người dùng (Nghị định 13/2023).</summary>
public record ExportUserDataQuery(Guid UserId) : IRequest<UserDataExportDto>;

using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Queries.GetVaccinePassport;

public class GetVaccinePassportQueryHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IVaccinePassportService passportService)
    : IRequestHandler<GetVaccinePassportQuery, byte[]>
{
    public async Task<byte[]> Handle(GetVaccinePassportQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User", userId);

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("HealthProfile", userId);

        var records = await db.VaccineRecords
            .AsNoTracking()
            .Where(r => r.HealthProfileId == profile.Id)
            .OrderBy(r => r.InjectionDate)
            .ToListAsync(ct);

        var dtos = records.Select(MapToDto).ToList();

        var data = new VaccinePassportData(
            FullName: $"{user.LastName} {user.FirstName}",
            Email: user.Email,
            BloodType: profile.BloodType?.ToString(),
            GeneratedAt: DateTime.UtcNow,
            Records: dtos);

        return passportService.Generate(data);
    }

    private static VaccineRecordDto MapToDto(VaccineRecord r)
    {
        var status = r.NextDueDate == null ? "completed"
            : r.IsOverdue() ? "overdue"
            : r.NextDueDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)) ? "upcoming"
            : "scheduled";

        return new VaccineRecordDto(
            r.Id, r.HealthProfileId, r.VaccineCatalogId, r.VaccineName,
            r.DoseNumber, r.InjectionDate, r.NextDueDate,
            r.Facility, r.LotNumber, r.AdministeredBy, r.Reaction,
            r.IsOverdue(), status, r.CreatedAt);
    }
}

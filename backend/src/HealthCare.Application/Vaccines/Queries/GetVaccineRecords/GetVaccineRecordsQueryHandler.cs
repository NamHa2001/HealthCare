using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineRecords;

public class GetVaccineRecordsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetVaccineRecordsQuery, IReadOnlyList<VaccineRecordDto>>
{
    public async Task<IReadOnlyList<VaccineRecordDto>> Handle(GetVaccineRecordsQuery request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        var records = await db.VaccineRecords
            .Where(r => r.HealthProfileId == profile.Id)
            .OrderByDescending(r => r.InjectionDate)
            .ToListAsync(ct);

        return records.Select(MapToDto).ToList();
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

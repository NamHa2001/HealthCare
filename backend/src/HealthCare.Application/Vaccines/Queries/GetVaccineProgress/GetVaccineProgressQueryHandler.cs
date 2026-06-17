using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineProgress;

public class GetVaccineProgressQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetVaccineProgressQuery, IReadOnlyList<VaccineProgressDto>>
{
    public async Task<IReadOnlyList<VaccineProgressDto>> Handle(GetVaccineProgressQuery request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        var records = await db.VaccineRecords
            .Where(r => r.HealthProfileId == profile.Id && r.VaccineCatalogId != null)
            .Include(r => r.VaccineCatalog)
            .OrderBy(r => r.InjectionDate)
            .ToListAsync(ct);

        var catalogs = await db.VaccineCatalog
            .Where(c => c.IsMandatory)
            .ToListAsync(ct);

        var result = new List<VaccineProgressDto>();

        foreach (var catalog in catalogs)
        {
            var catalogRecords = records.Where(r => r.VaccineCatalogId == catalog.Id).ToList();
            var dosesCompleted = catalogRecords.Count;
            var latestRecord = catalogRecords.MaxBy(r => r.DoseNumber);
            var nextDueDate = latestRecord?.NextDueDate;
            var isOverdue = latestRecord?.IsOverdue() ?? false;

            result.Add(new VaccineProgressDto(
                catalog.Id,
                catalog.Name,
                catalog.TotalDoses,
                dosesCompleted,
                nextDueDate,
                isOverdue,
                catalogRecords.Select(r => MapRecord(r)).ToList()));
        }

        return result;
    }

    private static VaccineRecordDto MapRecord(VaccineRecord r)
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

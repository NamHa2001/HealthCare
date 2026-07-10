using AutoMapper;
using AutoMapper.QueryableExtensions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Dashboard.Common;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Application.HealthProfiles.DTOs;
using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Dashboard.Queries;

// ─── Danh sách bệnh nhân của bác sĩ ──────────────────────────────────────────

public record GetMyPatientsQuery : IRequest<IReadOnlyList<PatientListItemDto>>;

public class GetMyPatientsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyPatientsQuery, IReadOnlyList<PatientListItemDto>>
{
    public async Task<IReadOnlyList<PatientListItemDto>> Handle(GetMyPatientsQuery request, CancellationToken ct)
    {
        var doctorUserId = currentUser.UserId!.Value;

        var links = await db.PatientDoctorLinks
            .Where(l => l.DoctorUserId == doctorUserId && l.Status == DoctorLinkStatus.Active)
            .ToListAsync(ct);

        var result = new List<PatientListItemDto>();
        foreach (var link in links)
        {
            var scopes = DoctorLinkHelper.GetScopes(link);
            var name = await DoctorLinkHelper.ResolveOwnerNameAsync(db, link.HealthProfileId, ct);

            // Từng trường chỉ điền khi nằm trong phạm vi bệnh nhân đồng ý
            decimal? bmi = null; DateTime? measuredAt = null;
            if (scopes.Contains(ShareScopes.Measurements))
            {
                var m = await db.HealthMeasurements
                    .Where(x => x.HealthProfileId == link.HealthProfileId)
                    .OrderByDescending(x => x.MeasuredAt)
                    .FirstOrDefaultAsync(ct);
                bmi = m?.Bmi;
                measuredAt = m?.MeasuredAt;
            }

            string? bp = null;
            if (scopes.Contains(ShareScopes.BloodPressure))
            {
                var b = await db.BloodPressureLogs
                    .Where(x => x.HealthProfileId == link.HealthProfileId)
                    .OrderByDescending(x => x.MeasuredAt)
                    .FirstOrDefaultAsync(ct);
                if (b is not null)
                {
                    bp = $"{b.Systolic}/{b.Diastolic}";
                    if (measuredAt is null || b.MeasuredAt > measuredAt) measuredAt = b.MeasuredAt;
                }
            }

            int critical = 0, warning = 0;
            if (scopes.Contains(ShareScopes.Measurements) || scopes.Contains(ShareScopes.BloodPressure))
            {
                var counts = await db.HealthAlerts
                    .Where(a => a.HealthProfileId == link.HealthProfileId && !a.IsAcknowledged)
                    .GroupBy(a => a.Severity)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                critical = counts.FirstOrDefault(c => c.Key == AlertSeverity.Critical)?.Count ?? 0;
                warning = counts.FirstOrDefault(c => c.Key == AlertSeverity.Warning)?.Count ?? 0;
            }

            DateOnly? lastVisit = null;
            if (scopes.Contains(ShareScopes.Visits))
            {
                lastVisit = await db.MedicalVisits
                    .Where(v => v.HealthProfileId == link.HealthProfileId)
                    .OrderByDescending(v => v.VisitDate)
                    .Select(v => (DateOnly?)v.VisitDate)
                    .FirstOrDefaultAsync(ct);
            }

            result.Add(new PatientListItemDto(
                link.Id, link.HealthProfileId, name, scopes, link.ConsentAt,
                critical, warning, bmi, bp, measuredAt, lastVisit));
        }

        // Bệnh nhân có cảnh báo critical lên đầu (DOCTOR_PORTAL.md §6.1)
        return result
            .OrderByDescending(p => p.CriticalAlerts)
            .ThenByDescending(p => p.WarningAlerts)
            .ThenBy(p => p.PatientName)
            .ToList();
    }
}

// ─── Tổng quan 1 bệnh nhân ───────────────────────────────────────────────────

public record GetPatientSummaryQuery(Guid HealthProfileId) : IRequest<PatientSummaryDto>;

public class GetPatientSummaryQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetPatientSummaryQuery, PatientSummaryDto>
{
    public async Task<PatientSummaryDto> Handle(GetPatientSummaryQuery request, CancellationToken ct)
    {
        var link = await DoctorAccess.EnsureLinkedAsync(
            db, currentUser.UserId!.Value, request.HealthProfileId, null, "doctor_patient_summary", ct);

        var scopes = DoctorLinkHelper.GetScopes(link);
        var name = await DoctorLinkHelper.ResolveOwnerNameAsync(db, request.HealthProfileId, ct);

        string? dob = null, gender = null, bloodType = null, allergies = null, chronic = null;
        if (scopes.Contains(ShareScopes.Profile))
        {
            var profile = await db.HealthProfiles.FirstAsync(p => p.Id == request.HealthProfileId, ct);
            bloodType = profile.BloodType?.ToString();
            allergies = profile.Allergies;
            chronic = profile.ChronicConditions;

            if (profile.FamilyMemberId is not null)
            {
                var member = await db.FamilyMembers.FirstAsync(m => m.Id == profile.FamilyMemberId, ct);
                dob = member.DateOfBirth.ToString("yyyy-MM-dd");
                gender = member.Gender;
            }
        }

        var alerts = new List<HealthAlertDto>();
        if (scopes.Contains(ShareScopes.Measurements) || scopes.Contains(ShareScopes.BloodPressure))
        {
            alerts = await db.HealthAlerts
                .Where(a => a.HealthProfileId == request.HealthProfileId && !a.IsAcknowledged)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ProjectTo<HealthAlertDto>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }

        return new PatientSummaryDto(
            request.HealthProfileId, name, scopes, link.ConsentAt,
            dob, gender, bloodType, allergies, chronic, alerts);
    }
}

// ─── Các mục dữ liệu chi tiết (guard theo đúng scope) ────────────────────────

public record GetPatientMeasurementsQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<HealthMeasurementDto>>;

public class GetPatientMeasurementsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetPatientMeasurementsQuery, IReadOnlyList<HealthMeasurementDto>>
{
    public async Task<IReadOnlyList<HealthMeasurementDto>> Handle(GetPatientMeasurementsQuery request, CancellationToken ct)
    {
        await DoctorAccess.EnsureLinkedAsync(db, currentUser.UserId!.Value,
            request.HealthProfileId, ShareScopes.Measurements, "doctor_patient_measurements", ct);

        return await db.HealthMeasurements
            .Where(m => m.HealthProfileId == request.HealthProfileId)
            .OrderByDescending(m => m.MeasuredAt)
            .Take(500)
            .ProjectTo<HealthMeasurementDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetPatientBloodPressureQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<BloodPressureLogDto>>;

public class GetPatientBloodPressureQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetPatientBloodPressureQuery, IReadOnlyList<BloodPressureLogDto>>
{
    public async Task<IReadOnlyList<BloodPressureLogDto>> Handle(GetPatientBloodPressureQuery request, CancellationToken ct)
    {
        await DoctorAccess.EnsureLinkedAsync(db, currentUser.UserId!.Value,
            request.HealthProfileId, ShareScopes.BloodPressure, "doctor_patient_blood_pressure", ct);

        return await db.BloodPressureLogs
            .Where(b => b.HealthProfileId == request.HealthProfileId)
            .OrderByDescending(b => b.MeasuredAt)
            .Take(500)
            .ProjectTo<BloodPressureLogDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetPatientVisitsQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<MedicalVisitListDto>>;

public class GetPatientVisitsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetPatientVisitsQuery, IReadOnlyList<MedicalVisitListDto>>
{
    public async Task<IReadOnlyList<MedicalVisitListDto>> Handle(GetPatientVisitsQuery request, CancellationToken ct)
    {
        await DoctorAccess.EnsureLinkedAsync(db, currentUser.UserId!.Value,
            request.HealthProfileId, ShareScopes.Visits, "doctor_patient_visits", ct);

        return await db.MedicalVisits
            .Where(v => v.HealthProfileId == request.HealthProfileId)
            .OrderByDescending(v => v.VisitDate)
            .Take(200)
            .ProjectTo<MedicalVisitListDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetPatientMedicationsQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<MedicationDto>>;

public class GetPatientMedicationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetPatientMedicationsQuery, IReadOnlyList<MedicationDto>>
{
    public async Task<IReadOnlyList<MedicationDto>> Handle(GetPatientMedicationsQuery request, CancellationToken ct)
    {
        await DoctorAccess.EnsureLinkedAsync(db, currentUser.UserId!.Value,
            request.HealthProfileId, ShareScopes.Medications, "doctor_patient_medications", ct);

        return await db.Medications
            .Where(m => m.HealthProfileId == request.HealthProfileId)
            .OrderByDescending(m => m.StartDate)
            .Take(200)
            .ProjectTo<MedicationDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetPatientVaccinesQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<VaccineRecordDto>>;

public class GetPatientVaccinesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetPatientVaccinesQuery, IReadOnlyList<VaccineRecordDto>>
{
    public async Task<IReadOnlyList<VaccineRecordDto>> Handle(GetPatientVaccinesQuery request, CancellationToken ct)
    {
        await DoctorAccess.EnsureLinkedAsync(db, currentUser.UserId!.Value,
            request.HealthProfileId, ShareScopes.Vaccines, "doctor_patient_vaccines", ct);

        var records = await db.VaccineRecords
            .Where(r => r.HealthProfileId == request.HealthProfileId)
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

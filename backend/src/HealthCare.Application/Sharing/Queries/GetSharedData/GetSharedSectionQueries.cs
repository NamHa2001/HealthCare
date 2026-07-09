using AutoMapper;
using AutoMapper.QueryableExtensions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.HealthProfiles.DTOs;
using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Queries.GetSharedData;

// Các endpoint dữ liệu read-only cho link chia sẻ — tái dùng DTO của từng module.

public record GetSharedMeasurementsQuery(string Token) : IRequest<IReadOnlyList<HealthMeasurementDto>>;

public class GetSharedMeasurementsQueryHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetSharedMeasurementsQuery, IReadOnlyList<HealthMeasurementDto>>
{
    public async Task<IReadOnlyList<HealthMeasurementDto>> Handle(GetSharedMeasurementsQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.Measurements, ct);

        return await db.HealthMeasurements
            .Where(m => m.HealthProfileId == grant.HealthProfileId)
            .OrderByDescending(m => m.MeasuredAt)
            .Take(500)
            .ProjectTo<HealthMeasurementDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetSharedBloodPressureQuery(string Token) : IRequest<IReadOnlyList<BloodPressureLogDto>>;

public class GetSharedBloodPressureQueryHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetSharedBloodPressureQuery, IReadOnlyList<BloodPressureLogDto>>
{
    public async Task<IReadOnlyList<BloodPressureLogDto>> Handle(GetSharedBloodPressureQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.BloodPressure, ct);

        return await db.BloodPressureLogs
            .Where(b => b.HealthProfileId == grant.HealthProfileId)
            .OrderByDescending(b => b.MeasuredAt)
            .Take(500)
            .ProjectTo<BloodPressureLogDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetSharedVisitsQuery(string Token) : IRequest<IReadOnlyList<MedicalVisitListDto>>;

public class GetSharedVisitsQueryHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetSharedVisitsQuery, IReadOnlyList<MedicalVisitListDto>>
{
    public async Task<IReadOnlyList<MedicalVisitListDto>> Handle(GetSharedVisitsQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.Visits, ct);

        return await db.MedicalVisits
            .Where(v => v.HealthProfileId == grant.HealthProfileId)
            .OrderByDescending(v => v.VisitDate)
            .Take(200)
            .ProjectTo<MedicalVisitListDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetSharedMedicationsQuery(string Token) : IRequest<IReadOnlyList<MedicationDto>>;

public class GetSharedMedicationsQueryHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetSharedMedicationsQuery, IReadOnlyList<MedicationDto>>
{
    public async Task<IReadOnlyList<MedicationDto>> Handle(GetSharedMedicationsQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.Medications, ct);

        return await db.Medications
            .Where(m => m.HealthProfileId == grant.HealthProfileId)
            .OrderByDescending(m => m.StartDate)
            .Take(200)
            .ProjectTo<MedicationDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetSharedVaccinesQuery(string Token) : IRequest<IReadOnlyList<VaccineRecordDto>>;

public class GetSharedVaccinesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSharedVaccinesQuery, IReadOnlyList<VaccineRecordDto>>
{
    public async Task<IReadOnlyList<VaccineRecordDto>> Handle(GetSharedVaccinesQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.Vaccines, ct);

        var records = await db.VaccineRecords
            .Where(r => r.HealthProfileId == grant.HealthProfileId)
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

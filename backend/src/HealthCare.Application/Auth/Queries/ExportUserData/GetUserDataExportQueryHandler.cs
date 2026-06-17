using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Queries.ExportUserData;

public class GetUserDataExportQueryHandler : IRequestHandler<ExportUserDataQuery, UserDataExportDto>
{
    private readonly IApplicationDbContext _db;

    public GetUserDataExportQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<UserDataExportDto> Handle(ExportUserDataQuery request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([request.UserId], ct)
            ?? throw new UnauthorizedAccessException();

        var profile = await _db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        var measurements = profile is null ? [] :
            await _db.HealthMeasurements
                .Where(m => m.HealthProfileId == profile.Id)
                .OrderBy(m => m.MeasuredAt)
                .Select(m => new ExportMeasurementDto(
                    m.MeasuredAt, m.WeightKg, m.HeightCm, m.Bmi,
                    m.HeartRateBpm, m.Spo2Percent, m.BodyTemperature, m.BloodGlucose))
                .ToListAsync(ct);

        var bpLogs = profile is null ? [] :
            await _db.BloodPressureLogs
                .Where(b => b.HealthProfileId == profile.Id)
                .OrderBy(b => b.MeasuredAt)
                .Select(b => new ExportBpDto(b.MeasuredAt, b.Systolic, b.Diastolic, b.Pulse))
                .ToListAsync(ct);

        var medications = profile is null ? [] :
            await _db.Medications
                .Where(m => m.HealthProfileId == profile.Id)
                .OrderBy(m => m.StartDate)
                .Select(m => new ExportMedicationDto(
                    m.DrugName, m.StartDate, m.EndDate, m.Instructions, null))
                .ToListAsync(ct);

        var vaccines = profile is null ? [] :
            await _db.VaccineRecords
                .Where(v => v.HealthProfileId == profile.Id)
                .OrderBy(v => v.InjectionDate)
                .Select(v => new ExportVaccineDto(
                    v.VaccineName, v.InjectionDate, v.NextDueDate, v.Reaction))
                .ToListAsync(ct);

        var visits = profile is null ? [] :
            await _db.MedicalVisits
                .Where(v => v.HealthProfileId == profile.Id)
                .OrderBy(v => v.VisitDate)
                .Select(v => new ExportVisitDto(
                    v.VisitDate, v.Diagnosis, v.DoctorName, v.FacilityName, v.Notes))
                .ToListAsync(ct);

        return new UserDataExportDto(
            Profile: new ExportProfileDto(
                user.Email, user.FirstName, user.LastName,
                user.PhoneNumber, user.CreatedAt, user.LastLoginAt),
            Measurements: measurements,
            BloodPressure: bpLogs,
            Medications: medications,
            Vaccines: vaccines,
            MedicalVisits: visits,
            ExportedAt: DateTime.UtcNow
        );
    }
}

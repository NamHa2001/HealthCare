using HealthCare.Application.Admin.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Admin.Queries.GetSystemStats;

public class GetSystemStatsQueryHandler : IRequestHandler<GetSystemStatsQuery, SystemStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetSystemStatsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SystemStatsDto> Handle(GetSystemStatsQuery request, CancellationToken ct)
    {
        var totalUsers        = await _context.Users.CountAsync(ct);
        var activeUsers       = await _context.Users.CountAsync(u => u.IsActive, ct);
        var totalProfiles     = await _context.HealthProfiles.CountAsync(ct);
        var totalMeasurements = await _context.HealthMeasurements.CountAsync(ct);
        var totalBpLogs       = await _context.BloodPressureLogs.CountAsync(ct);
        var totalVaccines     = await _context.VaccineRecords.CountAsync(ct);
        var totalMedications  = await _context.Medications.CountAsync(ct);
        var totalVisits       = await _context.MedicalVisits.CountAsync(ct);
        var totalReminders    = await _context.Reminders.CountAsync(ct);

        return new SystemStatsDto(
            totalUsers, activeUsers, totalProfiles, totalMeasurements,
            totalBpLogs, totalVaccines, totalMedications, totalVisits, totalReminders);
    }
}

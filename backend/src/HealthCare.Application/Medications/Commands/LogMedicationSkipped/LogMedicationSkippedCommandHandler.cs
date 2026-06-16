using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.LogMedicationSkipped;

public class LogMedicationSkippedCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<LogMedicationSkippedCommand, MedicationLogDto>
{
    public async Task<MedicationLogDto> Handle(LogMedicationSkippedCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var log = await db.MedicationLogs
            .Include(l => l.MedicationSchedule)
                .ThenInclude(s => s.Medication)
            .FirstOrDefaultAsync(l => l.Id == request.LogId, cancellationToken)
            ?? throw new NotFoundException("MedicationLog", request.LogId);

        if (log.MedicationSchedule.Medication.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        log.MarkSkipped(request.SkipReason);
        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<MedicationLogDto>(log);
    }
}

using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Vaccines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Commands.UpdateVaccineRecord;

public class UpdateVaccineRecordCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateVaccineRecordCommand, VaccineRecordDto>
{
    public async Task<VaccineRecordDto> Handle(UpdateVaccineRecordCommand request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        var record = await db.VaccineRecords
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("VaccineRecord", request.Id);

        record.Update(
            request.VaccineName,
            request.DoseNumber,
            request.InjectionDate,
            request.NextDueDate,
            request.Facility,
            request.LotNumber,
            request.AdministeredBy,
            request.Reaction);

        await db.SaveChangesAsync(ct);

        var status = record.NextDueDate == null ? "completed"
            : record.IsOverdue() ? "overdue"
            : record.NextDueDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)) ? "upcoming"
            : "scheduled";

        return new VaccineRecordDto(
            record.Id, record.HealthProfileId, record.VaccineCatalogId, record.VaccineName,
            record.DoseNumber, record.InjectionDate, record.NextDueDate,
            record.Facility, record.LotNumber, record.AdministeredBy, record.Reaction,
            record.IsOverdue(), status, record.CreatedAt);
    }
}

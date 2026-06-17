using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using HealthCare.Domain.Entities.Notifications;
using HealthCare.Domain.Entities.Vaccines;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Commands.CreateVaccineRecord;

public class CreateVaccineRecordCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IVaccineScheduleService vaccineScheduleService)
    : IRequestHandler<CreateVaccineRecordCommand, VaccineRecordDto>
{
    public async Task<VaccineRecordDto> Handle(CreateVaccineRecordCommand request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        DateOnly? nextDueDate = null;
        if (request.VaccineCatalogId.HasValue)
        {
            nextDueDate = await vaccineScheduleService.CalculateNextDueDateAsync(
                request.VaccineCatalogId.Value,
                request.DoseNumber,
                request.InjectionDate,
                ct);
        }

        var record = VaccineRecord.Create(
            profile.Id,
            request.VaccineName,
            request.DoseNumber,
            request.InjectionDate,
            nextDueDate,
            request.VaccineCatalogId,
            request.Facility,
            request.LotNumber,
            request.AdministeredBy,
            request.Reaction,
            request.DocumentId);

        db.VaccineRecords.Add(record);

        if (nextDueDate.HasValue)
        {
            var remindAt7 = nextDueDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)))
                .AddDays(-7);
            var remindAt1 = nextDueDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)))
                .AddDays(-1);

            if (remindAt7 > DateTime.UtcNow)
                db.Reminders.Add(Reminder.Create(
                    currentUser.UserId!.Value,
                    profile.Id,
                    ReminderType.Vaccine,
                    $"Nhắc tiêm {request.VaccineName} mũi {request.DoseNumber + 1} — còn 7 ngày",
                    remindAt7,
                    record.Id));

            if (remindAt1 > DateTime.UtcNow)
                db.Reminders.Add(Reminder.Create(
                    currentUser.UserId!.Value,
                    profile.Id,
                    ReminderType.Vaccine,
                    $"Nhắc tiêm {request.VaccineName} mũi {request.DoseNumber + 1} — ngày mai",
                    remindAt1,
                    record.Id));
        }

        await db.SaveChangesAsync(ct);

        return MapToDto(record);
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

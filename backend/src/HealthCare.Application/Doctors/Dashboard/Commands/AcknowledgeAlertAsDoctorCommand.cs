using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Dashboard.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Dashboard.Commands;

/// <summary>Thao tác ghi DUY NHẤT của bác sĩ ở MVP: đánh dấu đã xem xét cảnh báo (§6.2).</summary>
public record AcknowledgeAlertAsDoctorCommand(Guid HealthProfileId, Guid AlertId) : IRequest;

public class AcknowledgeAlertAsDoctorCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<AcknowledgeAlertAsDoctorCommand>
{
    public async Task Handle(AcknowledgeAlertAsDoctorCommand request, CancellationToken ct)
    {
        var doctorUserId = currentUser.UserId!.Value;

        await DoctorAccess.EnsureLinkedAsync(db, doctorUserId,
            request.HealthProfileId, null, "doctor_patient_alert", ct);

        var alert = await db.HealthAlerts.FirstOrDefaultAsync(
            a => a.Id == request.AlertId && a.HealthProfileId == request.HealthProfileId, ct)
            ?? throw new NotFoundException("HealthAlert", request.AlertId);

        alert.Acknowledge();
        await db.SaveChangesAsync(ct);

        // Ghi rõ acknowledged-by-doctor trong audit
        await DoctorAccess.WriteAuditAsync(db, doctorUserId,
            request.HealthProfileId, "doctor_patient_alert", "acknowledge", ct);
    }
}

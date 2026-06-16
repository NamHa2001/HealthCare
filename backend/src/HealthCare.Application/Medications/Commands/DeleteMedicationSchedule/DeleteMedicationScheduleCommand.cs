using MediatR;

namespace HealthCare.Application.Medications.Commands.DeleteMedicationSchedule;

public record DeleteMedicationScheduleCommand(Guid ScheduleId) : IRequest;

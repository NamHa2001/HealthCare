using MediatR;

namespace HealthCare.Application.Vaccines.Commands.DeleteVaccineRecord;

public record DeleteVaccineRecordCommand(Guid Id) : IRequest;

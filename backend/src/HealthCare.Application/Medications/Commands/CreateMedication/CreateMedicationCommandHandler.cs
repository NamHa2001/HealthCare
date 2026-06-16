using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Domain.Entities.Medications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.CreateMedication;

public class CreateMedicationCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<CreateMedicationCommand, MedicationDto>
{
    public async Task<MedicationDto> Handle(CreateMedicationCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var medication = Medication.Create(
            healthProfileId: profile.Id,
            drugName: request.DrugName,
            startDate: request.StartDate,
            medicalVisitId: request.MedicalVisitId,
            drugCatalogId: request.DrugCatalogId,
            strength: request.Strength,
            dosageForm: request.DosageForm,
            instructions: request.Instructions,
            endDate: request.EndDate,
            isOngoing: request.IsOngoing,
            ocrSourceDocId: request.OcrSourceDocId,
            confidenceScore: request.ConfidenceScore);

        db.Medications.Add(medication);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<MedicationDto>(medication);
    }
}

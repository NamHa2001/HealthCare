using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Ocr.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Ocr.Queries.GetOcrResult;

public class GetOcrResultQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetOcrResultQuery, OcrResultDto>
{
    public async Task<OcrResultDto> Handle(GetOcrResultQuery request, CancellationToken cancellationToken)
    {
        var document = await db.MedicalDocuments
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("MedicalDocument", request.DocumentId);

        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        if (document.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        return new OcrResultDto
        {
            DocumentId = document.Id,
            Status = document.OcrStatus.ToString(),
            RawText = document.OcrText,
            ConfidenceScore = null,
            Prescription = null
        };
    }
}

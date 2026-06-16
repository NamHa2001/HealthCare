using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Ocr.DTOs;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Ocr.Commands.ProcessOcr;

public class ProcessOcrCommandHandler(
    IApplicationDbContext db,
    IFileStorageService storage,
    IOcrService ocrService,
    ICurrentUser currentUser)
    : IRequestHandler<ProcessOcrCommand, OcrResultDto>
{
    public async Task<OcrResultDto> Handle(ProcessOcrCommand request, CancellationToken cancellationToken)
    {
        var document = await db.MedicalDocuments
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("MedicalDocument", request.DocumentId);

        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        if (document.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        document.SetOcrProcessing();
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var signedUrl = await storage.GetSignedUrlAsync(document.StorageKey, 10, cancellationToken);

            using var http = new HttpClient();
            var imageBytes = await http.GetByteArrayAsync(signedUrl, cancellationToken);
            using var stream = new MemoryStream(imageBytes);

            PrescriptionDataDto prescription;
            string rawText;
            decimal confidence;

            if (document.MimeType == "application/pdf")
            {
                var (text, conf) = await ocrService.ExtractTextAsync(stream, cancellationToken);
                rawText = text;
                confidence = (decimal)conf;
                prescription = new PrescriptionDataDto();
            }
            else
            {
                stream.Position = 0;
                prescription = await ocrService.ExtractPrescriptionAsync(stream, cancellationToken);
                rawText = string.Join("\n", prescription.Drugs.Select(d =>
                    $"{d.DrugName} {d.Strength} – {d.Dosage} {d.Frequency}".Trim()));
                confidence = prescription.Drugs.Count > 0
                    ? prescription.Drugs.Average(d => d.ConfidenceScore)
                    : 0m;
            }

            document.SetOcrDone(rawText);
            await db.SaveChangesAsync(cancellationToken);

            return new OcrResultDto
            {
                DocumentId = document.Id,
                Status = OcrStatus.Done.ToString(),
                RawText = rawText,
                ConfidenceScore = confidence,
                Prescription = prescription.Drugs.Count > 0 ? prescription : null
            };
        }
        catch
        {
            document.SetOcrFailed();
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}

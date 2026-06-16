using HealthCare.Application.Ocr.DTOs;

namespace HealthCare.Application.Common.Interfaces;

public interface IOcrService
{
    Task<(string RawText, double Confidence)> ExtractTextAsync(Stream imageStream, CancellationToken ct = default);
    Task<PrescriptionDataDto> ExtractPrescriptionAsync(Stream imageStream, CancellationToken ct = default);
}

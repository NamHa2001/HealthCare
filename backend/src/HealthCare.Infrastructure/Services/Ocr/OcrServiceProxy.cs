using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Ocr.DTOs;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.Services.Ocr;

/// <summary>
/// Circuit breaker: thử Google Vision trước, fallback sang EasyOCR nếu fail.
/// </summary>
public class OcrServiceProxy(
    GoogleVisionOcrService googleVision,
    EasyOcrService easyOcr,
    ILogger<OcrServiceProxy> logger)
    : IOcrService
{
    public async Task<(string RawText, double Confidence)> ExtractTextAsync(Stream imageStream, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("OCR: trying Google Vision...");
            return await googleVision.ExtractTextAsync(imageStream, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OCR: Google Vision failed, falling back to EasyOCR.");
            imageStream.Position = 0;
            return await easyOcr.ExtractTextAsync(imageStream, ct);
        }
    }

    public async Task<PrescriptionDataDto> ExtractPrescriptionAsync(Stream imageStream, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("OCR: trying Google Vision for prescription extraction...");
            return await googleVision.ExtractPrescriptionAsync(imageStream, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OCR: Google Vision failed, falling back to EasyOCR.");
            imageStream.Position = 0;
            return await easyOcr.ExtractPrescriptionAsync(imageStream, ct);
        }
    }
}

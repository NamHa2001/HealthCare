namespace HealthCare.Application.Common.Interfaces;

public interface IOcrService
{
    Task<(string RawText, double Confidence)> ExtractTextAsync(Stream imageStream, CancellationToken ct = default);
}
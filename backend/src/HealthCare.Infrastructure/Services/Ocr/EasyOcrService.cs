using System.Net.Http.Json;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Ocr.DTOs;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infrastructure.Services.Ocr;

public class EasyOcrService(IHttpClientFactory httpFactory, IConfiguration config, PrescriptionExtractor extractor)
    : IOcrService
{
    private readonly string _baseUrl = config["Ocr:EasyOcrUrl"] ?? "http://ocr-service:5000";

    public async Task<(string RawText, double Confidence)> ExtractTextAsync(Stream imageStream, CancellationToken ct = default)
    {
        using var client = httpFactory.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(imageStream), "file", "image.png");

        var response = await client.PostAsync($"{_baseUrl}/ocr", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EasyOcrResponse>(cancellationToken: ct);
        var rawText = result?.Text ?? string.Empty;
        var confidence = result?.Confidence ?? 0.5;
        return (rawText, confidence);
    }

    public async Task<PrescriptionDataDto> ExtractPrescriptionAsync(Stream imageStream, CancellationToken ct = default)
    {
        var preprocessed = new ImagePreprocessor();
        using var processed = await preprocessed.PreprocessAsync(imageStream, ct);

        var (rawText, confidence) = await ExtractTextAsync(processed, ct);
        var drugs = extractor.Extract(rawText, confidence);
        return new PrescriptionDataDto { Drugs = drugs };
    }

    private sealed class EasyOcrResponse
    {
        public string? Text { get; init; }
        public double Confidence { get; init; }
    }
}

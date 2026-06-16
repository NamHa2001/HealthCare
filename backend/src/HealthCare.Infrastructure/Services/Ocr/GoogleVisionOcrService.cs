using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Ocr.DTOs;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infrastructure.Services.Ocr;

public class GoogleVisionOcrService(IHttpClientFactory httpFactory, IConfiguration config, PrescriptionExtractor extractor)
    : IOcrService
{
    private readonly string _apiKey = config["Ocr:GoogleVisionApiKey"] ?? string.Empty;
    private const string VisionUrl = "https://vision.googleapis.com/v1/images:annotate";
    private readonly double _confidenceThreshold = double.Parse(config["Ocr:ConfidenceThreshold"] ?? "0.75");

    public async Task<(string RawText, double Confidence)> ExtractTextAsync(Stream imageStream, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, ct);
        var base64 = Convert.ToBase64String(ms.ToArray());

        var body = new
        {
            requests = new[]
            {
                new
                {
                    image = new { content = base64 },
                    features = new[] { new { type = "DOCUMENT_TEXT_DETECTION" } }
                }
            }
        };

        using var client = httpFactory.CreateClient();
        var json = JsonSerializer.Serialize(body);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{VisionUrl}?key={_apiKey}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(requestMessage, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var annotation = result.GetProperty("responses")[0];

        if (!annotation.TryGetProperty("fullTextAnnotation", out var textAnnotation))
            return (string.Empty, 0.0);

        var rawText = textAnnotation.GetProperty("text").GetString() ?? string.Empty;
        var confidence = ExtractConfidence(annotation);
        return (rawText, confidence);
    }

    public async Task<PrescriptionDataDto> ExtractPrescriptionAsync(Stream imageStream, CancellationToken ct = default)
    {
        var preprocessor = new ImagePreprocessor();
        using var processed = await preprocessor.PreprocessAsync(imageStream, ct);

        var (rawText, confidence) = await ExtractTextAsync(processed, ct);
        var drugs = extractor.Extract(rawText, confidence);
        return new PrescriptionDataDto { Drugs = drugs };
    }

    private static double ExtractConfidence(JsonElement annotation)
    {
        try
        {
            return annotation
                .GetProperty("fullTextAnnotation")
                .GetProperty("pages")[0]
                .GetProperty("confidence")
                .GetDouble();
        }
        catch
        {
            return 0.8; // default reasonable confidence khi không parse được
        }
    }
}

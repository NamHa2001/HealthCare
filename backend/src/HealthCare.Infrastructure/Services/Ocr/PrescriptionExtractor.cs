using System.Text.RegularExpressions;
using HealthCare.Application.Ocr.DTOs;

namespace HealthCare.Infrastructure.Services.Ocr;

public partial class PrescriptionExtractor
{
    // Regex patterns tiếng Việt (SRS §12.3)
    [GeneratedRegex(@"(\d+)\s*(viên|gói|ml|mg|mcg|g)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DosagePattern();

    [GeneratedRegex(@"(\d+)\s*lần\s*/\s*(ngày|tuần|tháng)", RegexOptions.IgnoreCase)]
    private static partial Regex FrequencyPattern();

    [GeneratedRegex(@"(sau|trước|trong khi)\s*ăn", RegexOptions.IgnoreCase)]
    private static partial Regex InstructionPattern();

    [GeneratedRegex(@"(?:trong|uống)\s*(\d+)\s*ngày", RegexOptions.IgnoreCase)]
    private static partial Regex DurationPattern();

    [GeneratedRegex(@"(\d+(?:[,\.]\d+)?)\s*(mg|mcg|g|ml|%)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StrengthPattern();

    public List<ExtractedDrugDto> Extract(string rawText, double overallConfidence)
    {
        var result = new List<ExtractedDrugDto>();
        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length < 3) continue;

            var dosageMatch = DosagePattern().Match(trimmed);
            var frequencyMatch = FrequencyPattern().Match(trimmed);
            var instructionMatch = InstructionPattern().Match(trimmed);
            var durationMatch = DurationPattern().Match(trimmed);
            var strengthMatch = StrengthPattern().Match(trimmed);

            // Chỉ tạo entry khi có ít nhất dosage hoặc frequency
            if (!dosageMatch.Success && !frequencyMatch.Success) continue;

            var drugName = ExtractDrugName(trimmed);
            if (string.IsNullOrWhiteSpace(drugName)) continue;

            result.Add(new ExtractedDrugDto
            {
                DrugName = drugName,
                Strength = strengthMatch.Success ? strengthMatch.Value : null,
                Dosage = dosageMatch.Success ? dosageMatch.Value : null,
                Frequency = frequencyMatch.Success ? frequencyMatch.Value : null,
                Duration = durationMatch.Success ? $"{durationMatch.Groups[1].Value} ngày" : null,
                Instructions = instructionMatch.Success ? instructionMatch.Value : null,
                ConfidenceScore = (decimal)overallConfidence
            });
        }

        return result;
    }

    private static string ExtractDrugName(string line)
    {
        // Lấy phần đầu dòng trước các số/đơn vị
        var match = Regex.Match(line, @"^([A-ZÀ-Ỹa-zà-ỹ][A-ZÀ-Ỹa-zà-ỹ\s\-]+?)(?:\s+\d|\s*$)");
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }
}

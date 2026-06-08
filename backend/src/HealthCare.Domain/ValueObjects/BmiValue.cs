namespace HealthCare.Domain.ValueObjects;

public record BmiValue
{
    public decimal Value { get; }
    public string Classification { get; }

    public BmiValue(decimal weightKg, decimal heightCm)
    {
        if (heightCm <= 0) throw new ArgumentException("Chiều cao không hợp lệ.");
        var heightM = heightCm / 100m;
        Value = Math.Round(weightKg / (heightM * heightM), 1);
        Classification = Value switch
        {
            < 18.5m => "Thiếu cân",
            < 23.0m => "Bình thường",
            < 25.0m => "Thừa cân",
            < 30.0m => "Béo phì độ I",
            _ => "Béo phì độ II"
        };
    }
}

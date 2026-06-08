namespace HealthCare.Domain.ValueObjects;

public record BloodPressureReading
{
    public int Systolic { get; }
    public int Diastolic { get; }
    public string Classification { get; }

    public BloodPressureReading(int systolic, int diastolic)
    {
        if (systolic <= 0 || diastolic <= 0) throw new ArgumentException("Giá trị huyết áp không hợp lệ.");
        Systolic = systolic;
        Diastolic = diastolic;

        Classification = (systolic, diastolic) switch
        {
            ( >= 180, _) or (_, >= 120) => "Khủng hoảng tăng huyết áp",
            ( >= 140, _) or (_, >= 90) => "Tăng huyết áp độ II",
            ( >= 130, _) or (_, >= 80) => "Tăng huyết áp độ I",
            ( >= 120, < 80) => "Tăng nhẹ",
            _ => "Bình thường"
        };
    }
}

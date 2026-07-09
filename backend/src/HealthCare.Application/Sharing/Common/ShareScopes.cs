namespace HealthCare.Application.Sharing.Common;

public static class ShareScopes
{
    public const string Profile = "profile";
    public const string Measurements = "measurements";
    public const string BloodPressure = "blood_pressure";
    public const string Visits = "visits";
    public const string Medications = "medications";
    public const string Vaccines = "vaccines";

    public static readonly IReadOnlyList<string> All =
        [Profile, Measurements, BloodPressure, Visits, Medications, Vaccines];

    public static bool IsValid(string scope) => All.Contains(scope);
}

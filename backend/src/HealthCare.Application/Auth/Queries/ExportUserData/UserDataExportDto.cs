namespace HealthCare.Application.Auth.Queries.ExportUserData;

public record UserDataExportDto(
    ExportProfileDto Profile,
    List<ExportMeasurementDto> Measurements,
    List<ExportBpDto> BloodPressure,
    List<ExportMedicationDto> Medications,
    List<ExportVaccineDto> Vaccines,
    List<ExportVisitDto> MedicalVisits,
    DateTime ExportedAt
);

public record ExportProfileDto(
    string Email, string FirstName, string LastName, string? PhoneNumber,
    DateTime CreatedAt, DateTime? LastLoginAt);

public record ExportMeasurementDto(
    DateTime MeasuredAt, decimal? WeightKg, decimal? HeightCm,
    decimal? Bmi, int? HeartRate, decimal? Spo2,
    decimal? Temperature, decimal? GlucoseMmol);

public record ExportBpDto(DateTime MeasuredAt, int Systolic, int Diastolic, int? HeartRate);

public record ExportMedicationDto(
    string DrugName, DateOnly StartDate, DateOnly? EndDate,
    string? Dosage, string? Notes);

public record ExportVaccineDto(
    string VaccineName, DateOnly? AdministeredDate,
    DateOnly? NextDueDate, string? Notes);

public record ExportVisitDto(
    DateOnly VisitDate, string? Diagnosis, string? DoctorName,
    string? Facility, string? Notes);

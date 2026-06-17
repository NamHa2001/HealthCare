namespace HealthCare.Application.Analytics.DTOs;

public record BpTrendPointDto(DateOnly Date, int Systolic, int Diastolic, int? Pulse);

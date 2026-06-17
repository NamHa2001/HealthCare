using HealthCare.Application.Vaccines.DTOs;

namespace HealthCare.Application.Common.Interfaces;

public interface IVaccinePassportService
{
    byte[] Generate(VaccinePassportData data);
}

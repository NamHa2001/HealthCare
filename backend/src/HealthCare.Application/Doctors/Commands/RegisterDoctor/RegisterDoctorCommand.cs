using HealthCare.Application.Doctors.DTOs;
using MediatR;

namespace HealthCare.Application.Doctors.Commands.RegisterDoctor;

/// <summary>Đăng ký làm bác sĩ. Nếu hồ sơ trước đó bị từ chối → nộp lại (resubmit).</summary>
public record RegisterDoctorCommand(
    string LicenseNumber,
    string Specialty,
    string Workplace,
    List<UploadedFile> LicenseFiles) : IRequest<DoctorProfileDto>;

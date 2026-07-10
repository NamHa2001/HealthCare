using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Commands.RegisterDoctor;

public class RegisterDoctorCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IFileStorageService storage)
    : IRequestHandler<RegisterDoctorCommand, DoctorProfileDto>
{
    public async Task<DoctorProfileDto> Handle(RegisterDoctorCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var license = request.LicenseNumber.Trim();

        // Số CCHN không được trùng với hồ sơ của user khác
        var licenseTaken = await db.DoctorProfiles.AnyAsync(
            d => d.LicenseNumber == license && d.UserId != userId, ct);
        if (licenseTaken)
            throw new ConflictException("Số chứng chỉ hành nghề đã được đăng ký bởi tài khoản khác.");

        var existing = await db.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId, ct);

        if (existing is not null && existing.Status != DoctorProfileStatus.Rejected)
            throw new ConflictException(existing.Status switch
            {
                DoctorProfileStatus.Pending => "Hồ sơ của bạn đang chờ duyệt.",
                DoctorProfileStatus.Approved => "Bạn đã là bác sĩ được xác minh.",
                _ => "Hồ sơ của bạn đang bị tạm ngưng — liên hệ quản trị viên.",
            });

        // Upload ảnh CCHN — chỉ truy cập được qua signed URL cấp cho admin
        var keys = new List<string>();
        foreach (var file in request.LicenseFiles)
        {
            var key = await storage.UploadAsync(file.Content, file.FileName, file.ContentType, ct);
            keys.Add(key);
        }
        var keysJson = JsonSerializer.Serialize(keys);

        DoctorProfile profile;
        if (existing is null)
        {
            profile = DoctorProfile.Create(userId, license, request.Specialty, request.Workplace, keysJson);
            db.DoctorProfiles.Add(profile);
        }
        else
        {
            existing.Resubmit(license, request.Specialty, request.Workplace, keysJson);
            profile = existing;
        }

        await db.SaveChangesAsync(ct);

        return new DoctorProfileDto(
            profile.Id, profile.LicenseNumber, profile.Specialty, profile.Workplace,
            profile.Status.ToString().ToLowerInvariant(), profile.RejectReason,
            profile.VerifiedAt, profile.CreatedAt);
    }
}

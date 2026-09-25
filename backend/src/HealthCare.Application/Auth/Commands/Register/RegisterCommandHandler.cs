using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Sharing.Common;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IApplicationDbContext db, IEmailService email, ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);
        if (exists) throw new ConflictException("Email đã được sử dụng.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName);

        _db.Users.Add(user);

        // SRS §1.5: Gán role mặc định 'user'
        var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "user", ct);
        if (userRole is not null)
            _db.UserRoles.Add(UserRole.Create(user.Id, userRole.Id));

        // SRS §1.1: Tự động tạo hồ sơ sức khỏe rỗng
        _db.HealthProfiles.Add(HealthProfile.CreateForUser(user.Id));

        var token = Guid.NewGuid().ToString("N");
        var verification = EmailVerification.Create(user.Id, ShareTokens.Hash(token), VerificationType.EmailVerify);
        _db.EmailVerifications.Add(verification);

        await _db.SaveChangesAsync(ct);

        try
        {
            await _email.SendEmailVerificationAsync(user.Email, user.FirstName, token, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể gửi email xác thực tới {Email}. Token: {Token}", user.Email, token);
        }

        return Result<Guid>.Success(user.Id);
    }
}
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Domain.Entities.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;

    public RegisterCommandHandler(IApplicationDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);
        if (exists) throw new ConflictException("Email đã được sử dụng.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName);

        _db.Users.Add(user);

        var token = Guid.NewGuid().ToString("N");
        var verification = EmailVerification.Create(user.Id, token);
        _db.EmailVerifications.Add(verification);

        await _db.SaveChangesAsync(ct);

        await _email.SendEmailVerificationAsync(user.Email, user.FirstName, token, ct);

        return Result<Guid>.Success(user.Id);
    }
}
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.UpdateHealthProfile;

public class UpdateHealthProfileCommandHandler : IRequestHandler<UpdateHealthProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryption;

    public UpdateHealthProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IEncryptionService encryption)
    {
        _context = context;
        _currentUser = currentUser;
        _encryption = encryption;
    }

    public async Task Handle(UpdateHealthProfileCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct);

        if (profile is null)
        {
            profile = HealthProfile.CreateForUser(_currentUser.UserId!.Value);
            _context.HealthProfiles.Add(profile);
        }

        byte[]? insuranceBytes = null;
        if (!string.IsNullOrEmpty(request.InsuranceNumberPlain))
            insuranceBytes = Convert.FromBase64String(_encryption.Encrypt(request.InsuranceNumberPlain));

        profile.Update(
            request.BloodType,
            request.Allergies,
            request.ChronicConditions,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            insuranceBytes,
            request.PrimaryDoctor,
            request.Notes);

        await _context.SaveChangesAsync(ct);
    }
}

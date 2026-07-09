using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Sharing.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Queries.GetSharedData;

public record GetSharedProfileQuery(string Token) : IRequest<SharedProfileDto>;

public class GetSharedProfileQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSharedProfileQuery, SharedProfileDto>
{
    public async Task<SharedProfileDto> Handle(GetSharedProfileQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, ShareScopes.Profile, ct);

        var profile = await db.HealthProfiles.FirstAsync(p => p.Id == grant.HealthProfileId, ct);

        string ownerName;
        string? dob = null, gender = null;

        if (profile.UserId is not null)
        {
            var user = await db.Users.FirstAsync(u => u.Id == profile.UserId, ct);
            ownerName = $"{user.FirstName} {user.LastName}";
        }
        else
        {
            var member = await db.FamilyMembers.FirstAsync(m => m.Id == profile.FamilyMemberId, ct);
            ownerName = member.FullName;
            dob = member.DateOfBirth.ToString("yyyy-MM-dd");
            gender = member.Gender;
        }

        // Cố ý KHÔNG trả InsuranceNumber (đã mã hóa) và Notes riêng tư
        return new SharedProfileDto(
            ownerName, dob, gender,
            profile.BloodType?.ToString(), profile.Allergies, profile.ChronicConditions,
            profile.EmergencyContactName, profile.EmergencyContactPhone,
            profile.PrimaryDoctor);
    }
}

using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Commands.DeleteMedicalVisit;

public class DeleteMedicalVisitCommandHandler : IRequestHandler<DeleteMedicalVisitCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteMedicalVisitCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteMedicalVisitCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var visit = await _context.MedicalVisits
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("MedicalVisit", request.Id);

        visit.SoftDelete();
        await _context.SaveChangesAsync(ct);
    }
}

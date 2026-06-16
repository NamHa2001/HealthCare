using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Queries.GetDocumentDownloadUrl;

public class GetDocumentDownloadUrlQueryHandler : IRequestHandler<GetDocumentDownloadUrlQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _storage;

    public GetDocumentDownloadUrlQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IFileStorageService storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<string> Handle(GetDocumentDownloadUrlQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var document = await _context.MedicalDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("MedicalDocument", request.DocumentId);

        // Signed URL hết hạn sau 60 phút per SRS §4.2
        return await _storage.GetSignedUrlAsync(document.StorageKey, expiryMinutes: 60, ct);
    }
}

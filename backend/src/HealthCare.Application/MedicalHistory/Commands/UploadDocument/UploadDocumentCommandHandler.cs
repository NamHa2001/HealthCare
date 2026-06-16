using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Domain.Entities.MedicalHistory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, MedicalDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _storage;
    private readonly IMapper _mapper;

    public UploadDocumentCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IFileStorageService storage,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<MedicalDocumentDto> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        // Kiểm tra visit thuộc profile nếu có chỉ định
        if (request.MedicalVisitId.HasValue)
        {
            var visitExists = await _context.MedicalVisits
                .AnyAsync(v => v.Id == request.MedicalVisitId.Value && v.HealthProfileId == profile.Id, ct);
            if (!visitExists)
                throw new NotFoundException("MedicalVisit", request.MedicalVisitId.Value);
        }

        var storageKey = await _storage.UploadAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            ct);

        var document = MedicalDocument.Create(
            profile.Id,
            _currentUser.UserId!.Value,
            request.FileName,
            request.FileSizeBytes,
            request.ContentType,
            storageKey,
            request.MedicalVisitId,
            documentType: request.DocumentType);

        _context.MedicalDocuments.Add(document);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<MedicalDocumentDto>(document);
    }
}

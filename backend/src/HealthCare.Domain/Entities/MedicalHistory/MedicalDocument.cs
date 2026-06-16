using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.MedicalHistory;

public class MedicalDocument : AuditableEntity
{
    public Guid? MedicalVisitId { get; private set; }
    public Guid HealthProfileId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;  // path trong MinIO
    public string? ThumbnailKey { get; private set; }
    public DocumentType? DocumentType { get; private set; }
    public OcrStatus OcrStatus { get; private set; } = OcrStatus.Pending;
    public string? OcrText { get; private set; }
    public Guid UploadedBy { get; private set; }

    public MedicalVisit? MedicalVisit { get; private set; }
    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;
    public Auth.User UploadedByUser { get; private set; } = null!;

    private MedicalDocument() { }

    public static MedicalDocument Create(
        Guid healthProfileId,
        Guid uploadedBy,
        string fileName,
        long fileSizeBytes,
        string mimeType,
        string storageKey,
        Guid? medicalVisitId = null,
        string? thumbnailKey = null,
        DocumentType? documentType = null) =>
        new()
        {
            HealthProfileId = healthProfileId,
            UploadedBy = uploadedBy,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            StorageKey = storageKey,
            MedicalVisitId = medicalVisitId,
            ThumbnailKey = thumbnailKey,
            DocumentType = documentType,
            OcrStatus = OcrStatus.Pending
        };

    public void LinkToVisit(Guid medicalVisitId) =>
        MedicalVisitId = medicalVisitId;

    public void SetOcrProcessing() =>
        OcrStatus = OcrStatus.Processing;

    public void SetOcrDone(string ocrText)
    {
        OcrStatus = OcrStatus.Done;
        OcrText = ocrText;
    }

    public void SetOcrFailed() =>
        OcrStatus = OcrStatus.Failed;

    public void SetThumbnail(string thumbnailKey) =>
        ThumbnailKey = thumbnailKey;
}

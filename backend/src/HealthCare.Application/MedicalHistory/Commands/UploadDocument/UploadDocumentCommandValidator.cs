using FluentValidation;

namespace HealthCare.Application.MedicalHistory.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/jpg",   // alias gửi bởi một số trình duyệt mobile
        "image/png",
        "image/webp",  // Android camera thường capture WebP
        "image/heic",
        "image/heif"
    ];

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB per SRS §4.2

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Tên file là bắt buộc.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage("Chỉ chấp nhận file PDF, JPG, PNG, WebP hoặc HEIC.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File không được rỗng.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File không được vượt quá 10MB.");
    }
}

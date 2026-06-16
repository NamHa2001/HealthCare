using FluentValidation;

namespace HealthCare.Application.MedicalHistory.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
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
            .WithMessage("Chỉ chấp nhận file PDF, JPG, PNG hoặc HEIC.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File không được rỗng.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File không được vượt quá 10MB.");
    }
}

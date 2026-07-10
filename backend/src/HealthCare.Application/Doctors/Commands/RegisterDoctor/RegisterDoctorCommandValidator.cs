using FluentValidation;

namespace HealthCare.Application.Doctors.Commands.RegisterDoctor;

public class RegisterDoctorCommandValidator : AbstractValidator<RegisterDoctorCommand>
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "application/pdf"];

    public RegisterDoctorCommandValidator()
    {
        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("Số chứng chỉ hành nghề là bắt buộc.")
            .Length(6, 20).WithMessage("Số CCHN phải từ 6-20 ký tự.");

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Chuyên khoa là bắt buộc.")
            .MaximumLength(100);

        RuleFor(x => x.Workplace)
            .NotEmpty().WithMessage("Nơi công tác là bắt buộc.")
            .MaximumLength(255);

        RuleFor(x => x.LicenseFiles)
            .NotEmpty().WithMessage("Phải upload ít nhất 1 ảnh chứng chỉ hành nghề.")
            .Must(f => f.Count <= 2).WithMessage("Tối đa 2 file.");

        RuleForEach(x => x.LicenseFiles).ChildRules(file =>
        {
            file.RuleFor(f => f.SizeBytes)
                .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File tối đa 10MB.");
            file.RuleFor(f => f.ContentType)
                .Must(t => AllowedContentTypes.Contains(t))
                .WithMessage("Chỉ chấp nhận JPG, PNG hoặc PDF.");
        });
    }
}

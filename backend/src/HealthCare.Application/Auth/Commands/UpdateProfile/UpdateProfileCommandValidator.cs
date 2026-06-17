using FluentValidation;

namespace HealthCare.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Họ không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(0|\+84)[3|5|7|8|9][0-9]{8}$")
            .WithMessage("Số điện thoại không đúng định dạng Việt Nam.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

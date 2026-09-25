using FluentValidation;

namespace HealthCare.Application.Common.Validators;

public static class PasswordRuleExtensions
{
    /// <summary>BUG-09: đồng bộ với frontend password-strength.validator.ts — hoa + thường + số + tối thiểu 8 ký tự.</summary>
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số.");
}

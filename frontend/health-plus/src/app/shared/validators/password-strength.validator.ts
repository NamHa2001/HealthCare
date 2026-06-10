import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Mật khẩu phải có: chữ hoa + chữ thường + chữ số + tối thiểu 8 ký tự.
 */
export function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value: string = control.value ?? '';
    if (!value) {
      return null;
    }

    const hasUpper = /[A-Z]/.test(value);
    const hasLower = /[a-z]/.test(value);
    const hasDigit = /[0-9]/.test(value);
    const hasMinLength = value.length >= 8;

    const valid = hasUpper && hasLower && hasDigit && hasMinLength;
    return valid ? null : { passwordStrength: { hasUpper, hasLower, hasDigit, hasMinLength } };
  };
}

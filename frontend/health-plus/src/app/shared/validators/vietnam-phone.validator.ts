import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function vietnamPhoneValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value: string = control.value ?? '';
    if (!value) {
      return null;
    }

    const valid = /^(0|\+84)[35789][0-9]{8}$/.test(value);
    return valid ? null : { vietnamPhone: true };
  };
}

import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Matches ASP.NET Identity options in Dental.Web (digit + uppercase, min 8). */
export const passwordPolicyValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = control.value as string | undefined;
  if (!value) {
    return null;
  }

  if (value.length < 8) {
    return { passwordPolicy: true };
  }

  if (!/[A-Z]/.test(value) || !/\d/.test(value)) {
    return { passwordPolicy: true };
  }

  return null;
};

import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
export function passwordNotContainUsernameValidator(
  usernameField: string
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    // If the parent is not set yet, return null (we can't validate)
    if (!control.parent) {
      return null;
    }
    const usernameControl = control.parent.get(usernameField);
    if (!usernameControl) {
      return null;
    }
    const username = usernameControl.value;
    const password = control.value;

    // Only perform check if both fields have values
    if (username && password && typeof password === 'string') {
      if (password.toLowerCase().includes(username.toLowerCase())) {
        return { containsUsername: true };
      }
    }
    return null;
  };
}

import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../core/auth/services/auth.service';
import { RegisterRequest } from '../../core/auth/models/register-request.model';

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword
    ? null
    : { passwordsDoNotMatch: true };
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly registerForm = this.formBuilder.nonNullable.group(
    {
      firstName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],
      lastName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.maxLength(255)
        ]
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(128)
        ]
      ],
      confirmPassword: [
        '',
        [
          Validators.required
        ]
      ]
    },
    {
      validators: passwordsMatchValidator
    }
  );

  isSubmitting = false;
  errorMessage = '';

  submit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const formValue = this.registerForm.getRawValue();

    const request: RegisterRequest = {
      firstName: formValue.firstName.trim(),
      lastName: formValue.lastName.trim(),
      email: formValue.email.trim(),
      password: formValue.password
    };

    this.authService
      .register(request)
      .pipe(
        finalize(() => {
          this.isSubmitting = false;
        })
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/login'], {
            queryParams: {
              registered: true
            }
          });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(error);
          this.isSubmitting = false;
          this.cdr.detectChanges();
        }
      });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API could not be reached. Make sure the backend is running.';
    }

    if (error.status === 400) {
      return this.extractApiMessage(
        error,
        'The registration information is invalid.'
      );
    }

    if (error.status === 409) {
      return 'An account with this email address already exists.';
    }

    if (error.status === 500) {
      return 'A server error occurred while creating the account.';
    }

    return this.extractApiMessage(
      error,
      'The account could not be created.'
    );
  }

  private extractApiMessage(
    error: HttpErrorResponse,
    fallbackMessage: string
  ): string {
    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    const validationErrors = error.error?.errors;

    if (validationErrors && typeof validationErrors === 'object') {
      const messages = Object.values(validationErrors)
        .flatMap(value => Array.isArray(value) ? value : [value])
        .filter((value): value is string => typeof value === 'string');

      if (messages.length > 0) {
        return messages[0];
      }
    }

    return error.error?.detail ??
      error.error?.title ??
      error.error?.message ??
      fallbackMessage;
  }
}
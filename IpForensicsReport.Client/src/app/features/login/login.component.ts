import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  readonly loginForm = this.formBuilder.nonNullable.group({
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
        Validators.maxLength(128)
      ]
    ]
  });

  isSubmitting = false;

  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const request = this.loginForm.getRawValue();

    this.authService
      .login(request)
      .pipe(
        finalize(() => {
          this.isSubmitting = false;
        })
      )
      .subscribe({
        next: response => {
          console.log(
            'Authenticated user:',
            response.user
          );

          void this.router.navigate(['/reports']);
        },
        error: error => {
          this.errorMessage = 
            this.getErrorMessage(error);
          this.isSubmitting = false;
          this.cdr.detectChanges();
        }
      });
  }

  private getErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'An unexpected error occurred.';
    }

    if (error.status === 0) {
      return 'The API could not be reached. Make sure the backend is running.';
    }

    if (error.status === 400) {
      return this.extractApiMessage(
        error,
        'Enter a valid email address and password.'
      );
    }

    if (error.status === 401) {
      return 'The email or password is incorrect.';
    }

    if (error.status === 500) {
      return 'A server error occurred. Please try again.';
    }

    return this.extractApiMessage(
      error,
      'Login failed. Please try again.'
    );
  }

  private extractApiMessage(
    error: HttpErrorResponse,
    fallbackMessage: string
  ): string {
    if (typeof error.error === 'string') {
      return error.error || fallbackMessage;
    }

    return error.error?.detail ??
      error.error?.title ??
      error.error?.message ??
      fallbackMessage;
  }
}
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';
import { ReportService } from './reports.service';
import { ReportSummary } from './models/report-summary.model';
import { ReportRequest } from './models/report-request.model';
import { AuthService } from '../../core/auth/services/auth.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly reports = signal<ReportSummary[]>([]);

  readonly isLoadingReports = signal(true);
  readonly isGeneratingReport = signal(false);

  readonly reportsErrorMessage = signal('');
  readonly generationErrorMessage = signal('');

  readonly ipAddressControl = new FormControl('', {
    nonNullable: true,
    validators: [
      Validators.required,
      Validators.maxLength(45)
    ]
  });

  ngOnInit(): void {
    this.loadReports();
  }

  logout(): void {
    this.authService.logout();

    void this.router.navigate(
      ['/login'],
      {
        replaceUrl: true
      }
    );
  }

  loadReports(): void {
    debugger;
    this.isLoadingReports.set(true);
    this.reportsErrorMessage.set('');

    this.reportService
      .getReports()
      .pipe(
        finalize(() => {
          this.isLoadingReports.set(false);
        })
      )
      .subscribe({
        next: reports => {
          this.reports.set(reports);
        },
        error: (error: HttpErrorResponse) => {
          this.handleReportsError(error);
        }
      });
  }

  generateReport(): void {
    this.ipAddressControl.markAsTouched();
    this.generationErrorMessage.set('');

    const ipAddress =
      this.ipAddressControl.value.trim();

    if (!ipAddress) {
      this.ipAddressControl.setErrors({
        required: true
      });

      return;
    }

    if (
      this.ipAddressControl.invalid ||
      this.isGeneratingReport()
    ) {
      return;
    }

    const request: ReportRequest = {
      ipAddress
    };

    this.isGeneratingReport.set(true);

    this.reportService
      .generateReport(request)
      .pipe(
        finalize(() => {
          this.isGeneratingReport.set(false);
        })
      )
      .subscribe({
        next: report => {
          this.ipAddressControl.reset();

          this.router.navigate([
            '/reports',
            report.id
          ]);
        },
        error: (error: HttpErrorResponse) => {
          this.handleGenerationError(error);
        }
      });
  }

  getRiskLevel(score: number): string {
    if (score >= 75) {
      return 'High risk';
    }

    if (score >= 25) {
      return 'Moderate risk';
    }

    return 'Low risk';
  }

  private handleReportsError(
    error: HttpErrorResponse
  ): void {
    if (error.status === 401) {
      this.reportsErrorMessage.set(
        'Your session has expired. Please sign in again.'
      );

      return;
    }

    this.reportsErrorMessage.set(
      'Saved reports could not be loaded.'
    );
  }

  private handleGenerationError(
    error: HttpErrorResponse
  ): void {
    if (error.status === 400) {
      this.generationErrorMessage.set(
        error.error?.detail ??
        error.error?.title ??
        'Enter a valid IP address.'
      );

      return;
    }

    if (error.status === 401) {
      this.generationErrorMessage.set(
        'Your session has expired. Please sign in again.'
      );

      return;
    }

    if (
      error.status === 502 ||
      error.status === 503
    ) {
      this.generationErrorMessage.set(
        'The external IP services are currently unavailable.'
      );

      return;
    }

    this.generationErrorMessage.set(
      'The report could not be generated.'
    );
  }
}
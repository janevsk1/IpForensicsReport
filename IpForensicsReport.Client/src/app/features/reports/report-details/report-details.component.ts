import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';

import { ReportDetails } from '../models/report-details.model';
import { ReportService } from '../reports.service';

@Component({
  selector: 'app-report-details',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink
  ],
  templateUrl: './report-details.component.html',
  styleUrl: './report-details.component.scss'
})
export class ReportDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly reportService = inject(ReportService);

  readonly report = signal<ReportDetails | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    const reportIdParameter =
      this.route.snapshot.paramMap.get('id');

    const reportId = Number(reportIdParameter);

    if (
      !reportIdParameter ||
      !Number.isSafeInteger(reportId) ||
      reportId <= 0
    ) {
      this.isLoading.set(false);
      this.errorMessage.set(
        'The report identifier is invalid.'
      );

      return;
    }

    this.loadReport(reportId);
  }

  loadReport(reportId: number): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.reportService
      .getReportById(reportId)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        })
      )
      .subscribe({
        next: report => {
          this.report.set(report);
        },
        error: (error: HttpErrorResponse) => {
          this.handleError(error);
        }
      });
  }

  goBack(): void {
    void this.router.navigate(['/reports']);
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

  getBooleanLabel(value: boolean): string {
    return value ? 'Yes' : 'No';
  }

  private handleError(
    error: HttpErrorResponse
  ): void {
    if (error.status === 401) {
      this.errorMessage.set(
        'Your session has expired. Please sign in again.'
      );

      return;
    }

    if (error.status === 404) {
      this.errorMessage.set(
        'The requested report was not found.'
      );

      return;
    }

    this.errorMessage.set(
      'The report details could not be loaded.'
    );
  }
}
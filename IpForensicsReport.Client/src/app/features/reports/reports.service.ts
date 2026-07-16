import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ReportSummary } from './models/report-summary.model';
import { ReportDetails } from './models/report-details.model';
import { ReportRequest } from './models/report-request.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly http = inject(HttpClient);

  private readonly reportsApiUrl = '/api/reports';

  getReports(): Observable<ReportSummary[]> {
    return this.http.get<ReportSummary[]>(
      `${this.reportsApiUrl}/reports`
    );
  }

  getReportById(
    reportId: number
  ): Observable<ReportDetails> {
    return this.http.get<ReportDetails>(
      `${this.reportsApiUrl}/${reportId}`
    );
  }

  generateReport(
    request: ReportRequest
  ): Observable<ReportDetails> {
    return this.http.post<ReportDetails>(
    `${this.reportsApiUrl}/generate`,
    request
    );
  }
}
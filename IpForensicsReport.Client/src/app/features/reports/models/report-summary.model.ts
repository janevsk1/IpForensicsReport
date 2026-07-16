export interface ReportSummary {
  id: number;
  ipAddress: string;
  abuseConfidenceScore: number;
  totalReports: number;
  country: string | null;
  city: string | null;
  createdOn: string;
}
import { ReportSummary } from "./report-summary.model";

export interface ReportDetails extends ReportSummary {
  lastReportedDate: string | null;
  continent: string | null;
  region: string | null;
  mobile: boolean;
  proxy: boolean;
  hosting: boolean;
  tor: boolean;
}
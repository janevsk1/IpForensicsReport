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

// export interface Report {
//   id: number;
//   ipAddress: string;
//   abuseConfidenceScore: number;
//   totalReports: number;
//   country: string | null;
//   city: string | null;
//   createdOn: string;
// }

// export interface ReportDetails {
//   id: number;
//   ipAddress: string;

//   abuseConfidenceScore: number;
//   totalReports: number;
//   lastReportedDate: string | null;

//   continent: string | null;
//   country: string | null;
//   region: string | null;
//   city: string | null;

//   mobile: boolean;
//   proxy: boolean;
//   hosting: boolean;
//   tor: boolean;

//   createdOn: string;
// }
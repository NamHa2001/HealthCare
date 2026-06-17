export interface HealthSummary {
  latestBmi: number | null;
  bmiLabel: string | null;
  latestWeightKg: number | null;
  latestGlucose: number | null;
  latestSystolic: number | null;
  latestDiastolic: number | null;
  bpLabel: string | null;
  upcomingRemindersCount: number;
  activeAlertsCount: number;
  vaccinesCompleted: number;
  medicationComplianceRate: number | null;
}

export interface TrendPoint {
  date: string;
  value: number;
}

export interface BpTrendPoint {
  date: string;
  systolic: number;
  diastolic: number;
  pulse: number | null;
}

export interface VaccineProgress {
  vaccineName: string;
  totalDoses: number;
  completedDoses: number;
  percentage: number;
}

export interface ComplianceWeek {
  weekLabel: string;
  complianceRate: number;
  total: number;
  taken: number;
}

export interface VisitFrequency {
  month: string;
  count: number;
}

export interface HealthScore {
  totalScore: number;
  bmiScore: number;
  bpScore: number;
  vaccineScore: number;
  medicationScore: number;
  bmiLabel: string | null;
  bpLabel: string | null;
  completedVaccines: number;
  totalVaccines: number;
  medicationComplianceRate: number | null;
  grade: string;
}

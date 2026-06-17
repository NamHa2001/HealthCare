export interface VaccineRecord {
  id: string;
  healthProfileId: string;
  vaccineCatalogId: string | null;
  vaccineName: string;
  doseNumber: number;
  injectionDate: string;
  nextDueDate: string | null;
  facility: string | null;
  lotNumber: string | null;
  administeredBy: string | null;
  reaction: string | null;
  isOverdue: boolean;
  status: 'completed' | 'overdue' | 'upcoming' | 'scheduled';
  createdAt: string;
}

export interface VaccineCatalog {
  id: string;
  name: string;
  shortName: string | null;
  diseasesCovered: string | null;
  totalDoses: number;
  isMandatory: boolean;
  ageStartMonths: number | null;
  notes: string | null;
  scheduleRules: VaccineScheduleRule[];
}

export interface VaccineScheduleRule {
  id: string;
  doseNumber: number;
  minAgeMonths: number | null;
  maxAgeMonths: number | null;
  minIntervalDays: number | null;
  recommendedIntervalDays: number | null;
}

export interface VaccineProgress {
  vaccineCatalogId: string;
  vaccineName: string;
  totalDoses: number;
  dosesCompleted: number;
  nextDueDate: string | null;
  isOverdue: boolean;
  records: VaccineRecord[];
}

export interface CreateVaccineRecordRequest {
  vaccineName: string;
  doseNumber: number;
  injectionDate: string;
  vaccineCatalogId: string | null;
  facility: string | null;
  lotNumber: string | null;
  administeredBy: string | null;
  reaction: string | null;
  documentId: string | null;
}

export type MedicationLogStatus = 'Pending' | 'Taken' | 'Skipped';

export interface MedicationSchedule {
  id: string;
  medicationId: string;
  scheduledTime: string; // 'HH:mm:ss'
  dosageAmount?: string | null;
  reminderEnabled: boolean;
  reminderMinutesBefore: number;
}

export interface MedicationLog {
  id: string;
  medicationScheduleId: string;
  scheduledAt: string;
  takenAt?: string | null;
  status: MedicationLogStatus;
  skipReason?: string | null;
  // from Include navigation (populated by backend MedicationLogDto)
  medicationName?: string;
  scheduledTime?: string;
}

export interface Medication {
  id: string;
  healthProfileId: string;
  medicalVisitId?: string | null;
  drugCatalogId?: string | null;
  drugName: string;
  strength?: string | null;
  dosageForm?: string | null;
  instructions?: string | null;
  startDate: string; // 'yyyy-MM-dd'
  endDate?: string | null;
  isOngoing: boolean;
  ocrSourceDocId?: string | null;
  confidenceScore?: number | null;
  schedules: MedicationSchedule[];
}

export interface CreateMedicationRequest {
  drugName: string;
  startDate: string;
  isOngoing: boolean;
  strength?: string | null;
  dosageForm?: string | null;
  instructions?: string | null;
  endDate?: string | null;
  medicalVisitId?: string | null;
  ocrSourceDocId?: string | null;
  confidenceScore?: number | null;
}

export interface UpdateMedicationRequest {
  drugName: string;
  startDate: string;
  isOngoing: boolean;
  strength?: string | null;
  dosageForm?: string | null;
  instructions?: string | null;
  endDate?: string | null;
}

export interface AddScheduleRequest {
  scheduledTime: string; // 'HH:mm:ss'
  dosageAmount?: string | null;
  reminderEnabled: boolean;
  reminderMinutesBefore: number;
}

export interface ComplianceReport {
  weekLabel: string;
  totalScheduled: number;
  totalTaken: number;
  compliancePercent: number;
}

export interface ExtractedDrug {
  drugName: string;
  strength?: string | null;
  dosageForm?: string | null;
  dosage?: string | null;
  frequency?: string | null;
  duration?: string | null;
  instructions?: string | null;
  confidenceScore: number;
  matchedDrugCatalogId?: string | null;
}

export interface OcrResult {
  documentId: string;
  status: string;
  rawText?: string | null;
  confidenceScore?: number | null;
  prescription?: { drugs: ExtractedDrug[] } | null;
}

export type ShareScope =
  | 'profile'
  | 'measurements'
  | 'blood_pressure'
  | 'visits'
  | 'medications'
  | 'vaccines';

export interface ShareGrant {
  id: string;
  healthProfileId: string;
  scope: ShareScope[];
  expiresAt: string;
  revokedAt: string | null;
  accessCount: number;
  lastAccessedAt: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateShareGrantRequest {
  healthProfileId: string;
  scope: ShareScope[];
  ttlHours: number;
}

/** Token gốc chỉ trả về 1 lần duy nhất khi tạo */
export interface CreateShareGrantResult {
  id: string;
  token: string;
  scope: ShareScope[];
  expiresAt: string;
}

export interface SharedMeta {
  ownerName: string;
  scope: ShareScope[];
  expiresAt: string;
}

export interface SharedProfile {
  ownerName: string;
  dateOfBirth: string | null;
  gender: string | null;
  bloodType: string | null;
  allergies: string | null;
  chronicConditions: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
  primaryDoctor: string | null;
}

export interface SharedMeasurement {
  id: string;
  measuredAt: string;
  weightKg: number | null;
  heightCm: number | null;
  bmi: number | null;
  heartRateBpm: number | null;
  bodyTemperature: number | null;
  bloodGlucose: number | null;
  spo2Percent: number | null;
  notes: string | null;
}

export interface SharedBloodPressure {
  id: string;
  measuredAt: string;
  systolic: number;
  diastolic: number;
  pulse: number | null;
}

export interface SharedVisit {
  id: string;
  visitDate: string;
  facilityName: string;
  doctorName: string | null;
  diagnosis: string;
  documentCount: number;
}

export interface SharedMedication {
  id: string;
  drugName: string;
  strength: string | null;
  instructions: string | null;
  startDate: string;
  endDate: string | null;
  isOngoing: boolean;
}

export interface SharedVaccine {
  id: string;
  vaccineName: string;
  doseNumber: number;
  injectionDate: string;
  nextDueDate: string | null;
  facility: string | null;
  status: string;
}

export const SCOPE_LABELS: Record<ShareScope, string> = {
  profile: 'Thông tin cơ bản',
  measurements: 'Chỉ số sức khỏe',
  blood_pressure: 'Huyết áp',
  visits: 'Lịch sử khám',
  medications: 'Thuốc đang dùng',
  vaccines: 'Tiêm chủng',
};

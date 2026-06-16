export type BloodType =
  | 'A_Positive' | 'A_Negative'
  | 'B_Positive' | 'B_Negative'
  | 'AB_Positive' | 'AB_Negative'
  | 'O_Positive' | 'O_Negative';

export const BLOOD_TYPE_LABELS: Record<BloodType, string> = {
  A_Positive: 'A+', A_Negative: 'A-',
  B_Positive: 'B+', B_Negative: 'B-',
  AB_Positive: 'AB+', AB_Negative: 'AB-',
  O_Positive: 'O+', O_Negative: 'O-',
};

export interface HealthProfile {
  id: string;
  userId?: string;
  familyMemberId?: string;
  bloodType?: BloodType;
  allergies?: string;
  chronicConditions?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  primaryDoctor?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateHealthProfileRequest {
  bloodType?: BloodType | null;
  allergies?: string | null;
  chronicConditions?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  insuranceNumberPlain?: string | null;
  primaryDoctor?: string | null;
  notes?: string | null;
}

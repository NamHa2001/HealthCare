export interface BloodPressureLog {
  id: string;
  healthProfileId: string;
  measuredAt: string;
  systolic: number;
  diastolic: number;
  pulse?: number;
  arm: string;
  position: string;
  notes?: string;
  createdAt: string;
}

export interface AddBloodPressureRequest {
  measuredAt: string;
  systolic: number;
  diastolic: number;
  pulse?: number | null;
  arm?: string;
  position?: string;
  notes?: string | null;
}

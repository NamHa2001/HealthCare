export interface HealthMeasurement {
  id: string;
  healthProfileId: string;
  measuredAt: string;
  weightKg?: number;
  heightCm?: number;
  bmi?: number;
  heartRateBpm?: number;
  bodyTemperature?: number;
  bloodGlucose?: number;
  spo2Percent?: number;
  notes?: string;
  source: string;
  createdAt: string;
}

export interface AddMeasurementRequest {
  measuredAt: string;
  weightKg?: number | null;
  heightCm?: number | null;
  heartRateBpm?: number | null;
  bodyTemperature?: number | null;
  bloodGlucose?: number | null;
  spo2Percent?: number | null;
  notes?: string | null;
  source?: string;
}

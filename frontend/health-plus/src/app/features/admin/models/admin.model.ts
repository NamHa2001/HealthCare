export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  isEmailVerified: boolean;
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  roles: string[];
}

export interface AuditLogItem {
  id: number;
  eventType: string;
  userId: string | null;
  userEmail: string | null;
  resource: string;
  action: string;
  entityId: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface SystemStats {
  totalUsers: number;
  activeUsers: number;
  totalHealthProfiles: number;
  totalMeasurements: number;
  totalBpLogs: number;
  totalVaccineRecords: number;
  totalMedications: number;
  totalMedicalVisits: number;
  totalReminders: number;
}

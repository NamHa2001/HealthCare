export type DoctorStatus = 'pending' | 'approved' | 'rejected' | 'suspended';

export interface DoctorProfile {
  id: string;
  licenseNumber: string;
  specialty: string;
  workplace: string;
  status: DoctorStatus;
  rejectReason: string | null;
  verifiedAt: string | null;
  createdAt: string;
}

export interface DoctorVerificationItem {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  licenseNumber: string;
  specialty: string;
  workplace: string;
  status: DoctorStatus;
  createdAt: string;
}

export interface DoctorVerificationDetail extends DoctorVerificationItem {
  phoneNumber: string | null;
  rejectReason: string | null;
  verifiedAt: string | null;
  licenseDocUrls: string[];
}

export type LinkStatus = 'pending' | 'active' | 'rejected' | 'revoked';

/** Liên kết nhìn từ phía bệnh nhân */
export interface DoctorLink {
  id: string;
  doctorUserId: string;
  doctorName: string;
  specialty: string;
  workplace: string;
  healthProfileId: string;
  profileOwnerName: string;
  initiatedBy: 'doctor' | 'patient';
  status: LinkStatus;
  consentAt: string | null;
  consentScope: string[];
  consentText: string | null;
  createdAt: string;
}

/** Liên kết nhìn từ phía bác sĩ */
export interface PatientLink {
  id: string;
  healthProfileId: string;
  patientName: string;
  initiatedBy: 'doctor' | 'patient';
  status: LinkStatus;
  consentAt: string | null;
  consentScope: string[];
  createdAt: string;
}

export interface DoctorSearchResult {
  userId: string;
  fullName: string;
  specialty: string;
  workplace: string;
  licenseNumber: string;
}

export const SPECIALTIES = [
  'Nội tổng quát', 'Nhi khoa', 'Sản phụ khoa', 'Tim mạch', 'Nội tiết',
  'Da liễu', 'Tai mũi họng', 'Mắt', 'Răng hàm mặt', 'Cơ xương khớp',
  'Thần kinh', 'Tiêu hóa', 'Hô hấp', 'Ung bướu', 'Y học gia đình', 'Khác',
];

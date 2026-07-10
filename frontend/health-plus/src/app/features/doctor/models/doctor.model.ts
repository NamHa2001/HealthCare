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

export const SPECIALTIES = [
  'Nội tổng quát', 'Nhi khoa', 'Sản phụ khoa', 'Tim mạch', 'Nội tiết',
  'Da liễu', 'Tai mũi họng', 'Mắt', 'Răng hàm mặt', 'Cơ xương khớp',
  'Thần kinh', 'Tiêu hóa', 'Hô hấp', 'Ung bướu', 'Y học gia đình', 'Khác',
];

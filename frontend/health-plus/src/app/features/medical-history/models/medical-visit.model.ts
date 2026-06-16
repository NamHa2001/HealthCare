/** Loại tài liệu y tế — khớp enum DocumentType backend. */
export type DocumentType = 'Prescription' | 'TestResult' | 'Xray' | 'Report' | 'Other';

export const DOCUMENT_TYPE_LABELS: Record<DocumentType, string> = {
  Prescription: 'Toa thuốc',
  TestResult: 'Kết quả xét nghiệm',
  Xray: 'X-quang',
  Report: 'Báo cáo',
  Other: 'Khác',
};

/** Trạng thái OCR — khớp enum OcrStatus backend. */
export type OcrStatus = 'Pending' | 'Processing' | 'Done' | 'Failed';

export interface MedicalDocument {
  id: string;
  medicalVisitId?: string | null;
  healthProfileId: string;
  fileName: string;
  fileSizeBytes: number;
  mimeType: string;
  documentType?: DocumentType | null;
  ocrStatus: OcrStatus;
  createdAt: string;
}

/** Bản rút gọn dùng cho danh sách (khớp MedicalVisitListDto). */
export interface MedicalVisitListItem {
  id: string;
  visitDate: string;       // 'yyyy-MM-dd'
  facilityName: string;
  doctorName?: string | null;
  diagnosis: string;
  documentCount: number;
  createdAt: string;
}

/** Bản đầy đủ (khớp MedicalVisitDto). */
export interface MedicalVisit {
  id: string;
  healthProfileId: string;
  visitDate: string;       // 'yyyy-MM-dd'
  facilityName: string;
  doctorName?: string | null;
  chiefComplaint: string;
  diagnosis: string;
  icd10Code?: string | null;
  treatment?: string | null;
  followUpDate?: string | null;
  cost?: number | null;
  notes?: string | null;
  createdAt: string;
  updatedAt: string;
  documents: MedicalDocument[];
}

export interface CreateMedicalVisitRequest {
  visitDate: string;
  facilityName: string;
  chiefComplaint: string;
  diagnosis: string;
  doctorName?: string | null;
  icd10Code?: string | null;
  treatment?: string | null;
  followUpDate?: string | null;
  cost?: number | null;
  notes?: string | null;
}

export type UpdateMedicalVisitRequest = CreateMedicalVisitRequest;

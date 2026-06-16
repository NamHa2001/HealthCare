import { Routes } from '@angular/router';

export const OCR_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'upload',
    pathMatch: 'full',
  },
  {
    path: 'upload',
    loadComponent: () =>
      import('./ocr-upload/ocr-upload.component').then(m => m.OcrUploadComponent),
    title: 'Quét đơn thuốc — Health+',
  },
  {
    path: 'review/:docId',
    loadComponent: () =>
      import('./ocr-review/ocr-review.component').then(m => m.OcrReviewComponent),
    title: 'Xem kết quả OCR — Health+',
  },
];

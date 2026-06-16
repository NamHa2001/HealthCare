import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MedicalHistoryStore } from '../medical-history.store';
import { DocumentViewerComponent } from '../document-viewer/document-viewer.component';
import { DOCUMENT_TYPE_LABELS, DocumentType, MedicalDocument } from '../models/medical-visit.model';

@Component({
  selector: 'app-visit-detail',
  standalone: true,
  imports: [
    DatePipe, DecimalPipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './visit-detail.component.html',
})
export class VisitDetailComponent implements OnInit {
  protected readonly store = inject(MedicalHistoryStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly uploading = signal(false);
  protected visitId = '';

  ngOnInit(): void {
    this.visitId = this.route.snapshot.paramMap.get('id') ?? '';
    if (this.visitId) this.store.loadVisitById(this.visitId);
  }

  protected docTypeLabel(type?: DocumentType | null): string {
    return type ? DOCUMENT_TYPE_LABELS[type] : '—';
  }

  protected formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  protected async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Giới hạn 10MB per SRS §4.2
    if (file.size > 10 * 1024 * 1024) {
      this.snackBar.open('File vượt quá 10MB.', 'Đóng', { duration: 4000 });
      input.value = '';
      return;
    }

    this.uploading.set(true);
    try {
      await this.store.uploadDocument(file, { medicalVisitId: this.visitId });
      this.snackBar.open('Đã tải lên tài liệu.', 'Đóng', { duration: 3000 });
    } catch {
      this.snackBar.open('Tải lên thất bại. Vui lòng thử lại.', 'Đóng', { duration: 4000 });
    } finally {
      this.uploading.set(false);
      input.value = '';
    }
  }

  protected viewDocument(doc: MedicalDocument): void {
    this.dialog.open(DocumentViewerComponent, {
      data: doc,
      width: '800px',
      maxWidth: '95vw',
    });
  }

  protected deleteDocument(doc: MedicalDocument): void {
    if (confirm(`Xóa tài liệu "${doc.fileName}"?`)) {
      this.store.deleteDocument(doc.id);
    }
  }

  protected deleteVisit(): void {
    const v = this.store.currentVisit();
    if (!v) return;
    if (confirm(`Xóa lần khám tại "${v.facilityName}"?`)) {
      this.store.deleteVisit(v.id).then(() => this.router.navigate(['/medical-history/visits']));
    }
  }
}

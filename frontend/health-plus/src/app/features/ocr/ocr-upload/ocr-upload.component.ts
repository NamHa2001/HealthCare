import { Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom, map } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import { MedicalDocument } from '../../medical-history/models/medical-visit.model';
import { MedicationsStore } from '../../medications/medications.store';

type UploadState = 'idle' | 'uploading' | 'processing' | 'done' | 'error';

@Component({
  selector: 'app-ocr-upload',
  standalone: true,
  imports: [
    DecimalPipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressBarModule, MatProgressSpinnerModule,
  ],
  templateUrl: './ocr-upload.component.html',
})
export class OcrUploadComponent {
  @ViewChild('fileInput') private readonly fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('cameraInput') private readonly cameraInput!: ElementRef<HTMLInputElement>;

  private readonly http = inject(HttpClient);
  private readonly store = inject(MedicationsStore);
  private readonly router = inject(Router);

  protected readonly state = signal<UploadState>('idle');
  protected readonly errorMsg = signal<string | null>(null);
  protected readonly preview = signal<string | null>(null);
  protected readonly selectedFile = signal<File | null>(null);

  protected onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.setFile(file);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files?.[0];
    if (file) this.setFile(file);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  private setFile(file: File): void {
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/heic', 'image/heif', 'application/pdf'];
    const allowedExts = ['jpg', 'jpeg', 'png', 'webp', 'heic', 'heif', 'pdf'];
    const ext = file.name.split('.').pop()?.toLowerCase() ?? '';

    // file.type có thể rỗng trên một số thiết bị mobile/camera
    const typeOk = file.type ? allowedTypes.includes(file.type) : false;
    const extOk = allowedExts.includes(ext);

    if (!typeOk && !extOk) {
      this.errorMsg.set('Chỉ chấp nhận ảnh (JPEG, PNG, WebP) hoặc PDF.');
      return;
    }

    this.selectedFile.set(file);
    this.errorMsg.set(null);
    this.state.set('idle');

    const isImage = file.type ? file.type.startsWith('image/') : allowedExts.filter(e => e !== 'pdf').includes(ext);
    if (isImage) {
      const reader = new FileReader();
      reader.onload = () => this.preview.set(reader.result as string);
      reader.readAsDataURL(file);
    } else {
      this.preview.set(null);
    }
  }

  protected triggerFile(): void {
    this.fileInput.nativeElement.click();
  }

  protected triggerCamera(): void {
    this.cameraInput.nativeElement.click();
  }

  protected removeFile(): void {
    this.selectedFile.set(null);
    this.preview.set(null);
    this.state.set('idle');
    this.errorMsg.set(null);
  }

  protected async upload(): Promise<void> {
    const file = this.selectedFile();
    if (!file) return;

    this.state.set('uploading');
    this.errorMsg.set(null);

    try {
      // Step 1: upload document
      const formData = new FormData();
      formData.append('file', file, file.name);
      formData.append('documentType', 'Prescription');

      const doc = await firstValueFrom(
        this.http
          .post<ApiResponse<MedicalDocument>>(`${environment.apiUrl}/Documents/upload`, formData)
          .pipe(map(r => r.data))
      );

      // Step 2: trigger OCR
      this.state.set('processing');
      await this.store.triggerOcr(doc.id);

      // Step 3: navigate to review
      this.state.set('done');
      await this.router.navigate(['/ocr/review', doc.id]);
    } catch (err: unknown) {
      this.state.set('error');
      const msg = (err as { error?: { message?: string } })?.error?.message;
      this.errorMsg.set(msg ?? 'Upload thất bại. Vui lòng thử lại.');
    }
  }
}

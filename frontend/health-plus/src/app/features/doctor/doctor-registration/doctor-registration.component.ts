import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiService } from '../../../core/services/api.service';
import { ApiResponse } from '../../../core/models/api-response.model';
import { DoctorProfile, SPECIALTIES } from '../models/doctor.model';

@Component({
  selector: 'app-doctor-registration',
  standalone: true,
  imports: [
    DatePipe, ReactiveFormsModule,
    MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule,
    MatProgressSpinnerModule, MatSelectModule,
  ],
  templateUrl: './doctor-registration.component.html',
})
export class DoctorRegistrationComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly specialties = SPECIALTIES;
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly profile = signal<DoctorProfile | null>(null);
  protected readonly files = signal<File[]>([]);
  protected readonly previews = signal<string[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    licenseNumber: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(20)]],
    specialty: ['', Validators.required],
    workplace: ['', [Validators.required, Validators.maxLength(255)]],
  });

  async ngOnInit(): Promise<void> {
    try {
      const profile = await firstValueFrom(this.api.get<DoctorProfile | null>('Doctor/me'));
      this.profile.set(profile);
      if (profile?.status === 'rejected') {
        this.form.patchValue({
          licenseNumber: profile.licenseNumber,
          specialty: profile.specialty,
          workplace: profile.workplace,
        });
      }
    } finally {
      this.loading.set(false);
    }
  }

  /** Hiện form khi: chưa đăng ký, hoặc bị từ chối (nộp lại) */
  protected showForm(): boolean {
    const p = this.profile();
    return p === null || p.status === 'rejected';
  }

  protected onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selected = Array.from(input.files ?? []).slice(0, 2);
    this.files.set(selected);
    this.previews.set([]);
    for (const f of selected) {
      if (f.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = () => this.previews.update(p => [...p, reader.result as string]);
        reader.readAsDataURL(f);
      }
    }
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.files().length === 0) {
      this.snackBar.open('Điền đủ thông tin và chọn ít nhất 1 ảnh chứng chỉ hành nghề.', 'Đóng', { duration: 4000 });
      return;
    }
    this.submitting.set(true);
    try {
      const fd = new FormData();
      fd.append('licenseNumber', this.form.getRawValue().licenseNumber);
      fd.append('specialty', this.form.getRawValue().specialty);
      fd.append('workplace', this.form.getRawValue().workplace);
      for (const f of this.files()) fd.append('licenseFiles', f);

      const res = await firstValueFrom(
        this.http.post<ApiResponse<DoctorProfile>>(`${environment.apiUrl}/Doctor/register`, fd));
      this.profile.set(res.data);
      this.snackBar.open('Đã nộp hồ sơ — chờ quản trị viên duyệt.', undefined, { duration: 4000 });
    } catch {
      this.snackBar.open('Nộp hồ sơ thất bại. Kiểm tra số CCHN chưa được dùng và file dưới 10MB.', 'Đóng', { duration: 5000 });
    } finally {
      this.submitting.set(false);
    }
  }

  protected statusInfo(): { label: string; cls: string; icon: string } {
    switch (this.profile()?.status) {
      case 'pending': return { label: 'Đang chờ duyệt', cls: 'badge-warning', icon: 'hourglass_top' };
      case 'approved': return { label: 'Đã xác minh', cls: 'badge-success', icon: 'verified' };
      case 'suspended': return { label: 'Tạm ngưng', cls: 'badge-danger', icon: 'block' };
      default: return { label: 'Bị từ chối', cls: 'badge-danger', icon: 'cancel' };
    }
  }
}

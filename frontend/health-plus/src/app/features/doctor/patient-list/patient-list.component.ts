import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { SCOPE_LABELS, ShareScope } from '../../sharing/models/share.model';
import { PatientListItem } from '../models/doctor.model';

/** Danh sách bệnh nhân của bác sĩ — bệnh nhân có cảnh báo critical lên đầu. */
@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './patient-list.component.html',
})
export class PatientListComponent implements OnInit {
  private readonly api = inject(ApiService);

  protected readonly loading = signal(true);
  protected readonly patients = signal<PatientListItem[]>([]);

  async ngOnInit(): Promise<void> {
    try {
      this.patients.set(await firstValueFrom(this.api.get<PatientListItem[]>('Doctor/patients')));
    } finally {
      this.loading.set(false);
    }
  }

  protected scopeLabel(scope: string): string {
    return SCOPE_LABELS[scope as ShareScope] ?? scope;
  }
}

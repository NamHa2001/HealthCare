import { Component, OnInit, inject, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MedicationsStore } from '../medications.store';
import { Medication, MedicationSchedule } from '../models/medication.model';

@Component({
  selector: 'app-medication-schedule',
  standalone: true,
  imports: [
    SlicePipe, ReactiveFormsModule, RouterLink,
    MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatIconModule,
    MatInputModule, MatProgressSpinnerModule, MatSelectModule,
    MatSlideToggleModule, MatTooltipModule,
  ],
  templateUrl: './medication-schedule.component.html',
})
export class MedicationScheduleComponent implements OnInit {
  private readonly store = inject(MedicationsStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly medication = signal<Medication | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    scheduledTime: ['08:00', Validators.required],
    dosageAmount: [''],
    reminderEnabled: [true],
    reminderMinutesBefore: [15],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    // Try from store first; if not loaded, load all
    const found = this.store.medications().find(m => m.id === id) ?? null;
    this.medication.set(found);
    if (!found) {
      this.store.loadMedications().then(() => {
        this.medication.set(this.store.medications().find(m => m.id === id) ?? null);
      });
    }
  }

  protected async addSchedule(): Promise<void> {
    if (this.form.invalid || !this.medication()) return;
    this.saving.set(true);
    try {
      const raw = this.form.getRawValue();
      await this.store.addSchedule(this.medication()!.id, {
        scheduledTime: raw.scheduledTime + ':00', // ensure HH:mm:ss
        dosageAmount: raw.dosageAmount || null,
        reminderEnabled: raw.reminderEnabled,
        reminderMinutesBefore: Number(raw.reminderMinutesBefore),
      });
      // Reset time to default for next entry
      this.form.patchValue({ scheduledTime: '08:00', dosageAmount: '' });
    } finally {
      this.saving.set(false);
    }
  }

  protected async removeSchedule(s: MedicationSchedule): Promise<void> {
    if (!confirm(`Xóa lịch uống lúc ${s.scheduledTime.substring(0, 5)}?`)) return;
    await this.store.deleteSchedule(this.medication()!.id, s.id);
    // Refresh local
    this.medication.set(this.store.medications().find(m => m.id === this.medication()!.id) ?? null);
  }

  protected done(): void {
    this.router.navigate(['/medications']);
  }
}

import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HealthRecordsStore } from '../health-records.store';

@Component({
  selector: 'app-measurement-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
  ],
  templateUrl: './measurement-form.component.html',
})
export class MeasurementFormComponent {
  private readonly store = inject(HealthRecordsStore);
  private readonly dialogRef = inject(MatDialogRef<MeasurementFormComponent>);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);

  protected readonly form = this.fb.group({
    measuredAt:      [new Date().toISOString().slice(0, 16), Validators.required],
    weightKg:        [null as number | null, [Validators.min(1), Validators.max(500)]],
    heightCm:        [null as number | null, [Validators.min(30), Validators.max(300)]],
    heartRateBpm:    [null as number | null, [Validators.min(20), Validators.max(300)]],
    bodyTemperature: [null as number | null, [Validators.min(30), Validators.max(45)]],
    bloodGlucose:    [null as number | null, [Validators.min(0), Validators.max(50)]],
    spo2Percent:     [null as number | null, [Validators.min(50), Validators.max(100)]],
    notes:           [''],
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const raw = this.form.getRawValue();
    const hasValue = [
      raw.weightKg, raw.heightCm, raw.heartRateBpm,
      raw.bodyTemperature, raw.bloodGlucose, raw.spo2Percent,
    ].some(v => v != null);
    if (!hasValue) {
      alert('Vui lòng nhập ít nhất một chỉ số.');
      return;
    }
    this.saving.set(true);
    try {
      await this.store.addMeasurement({
        measuredAt:      new Date(raw.measuredAt!).toISOString(),
        weightKg:        raw.weightKg,
        heightCm:        raw.heightCm,
        heartRateBpm:    raw.heartRateBpm,
        bodyTemperature: raw.bodyTemperature,
        bloodGlucose:    raw.bloodGlucose,
        spo2Percent:     raw.spo2Percent,
        notes:           raw.notes || null,
      });
      this.dialogRef.close(true);
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}

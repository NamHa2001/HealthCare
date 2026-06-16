import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HealthRecordsStore } from '../health-records.store';

@Component({
  selector: 'app-bp-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './bp-form.component.html',
})
export class BpFormComponent {
  private readonly store = inject(HealthRecordsStore);
  private readonly dialogRef = inject(MatDialogRef<BpFormComponent>);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);

  protected readonly form = this.fb.group({
    measuredAt: [new Date().toISOString().slice(0, 16), Validators.required],
    systolic:   [null as number | null, [Validators.required, Validators.min(50), Validators.max(300)]],
    diastolic:  [null as number | null, [Validators.required, Validators.min(30), Validators.max(200)]],
    pulse:      [null as number | null, [Validators.min(20), Validators.max(300)]],
    arm:        ['left'],
    position:   ['sitting'],
    notes:      [''],
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const raw = this.form.getRawValue();
    try {
      await this.store.addBpLog({
        measuredAt: new Date(raw.measuredAt!).toISOString(),
        systolic:   raw.systolic!,
        diastolic:  raw.diastolic!,
        pulse:      raw.pulse,
        arm:        raw.arm ?? 'left',
        position:   raw.position ?? 'sitting',
        notes:      raw.notes || null,
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

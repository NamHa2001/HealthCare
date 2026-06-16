import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HealthRecordsStore } from '../health-records.store';
import { BloodType, BLOOD_TYPE_LABELS } from '../models/health-profile.model';
import { vietnamPhoneValidator } from '../../../shared/validators/vietnam-phone.validator';

@Component({
  selector: 'app-health-profile-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './health-profile-form.component.html',
})
export class HealthProfileFormComponent implements OnInit {
  private readonly store = inject(HealthRecordsStore);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly bloodTypeOptions = Object.entries(BLOOD_TYPE_LABELS) as [BloodType, string][];
  private formPatched = false;

  protected readonly form = this.fb.group({
    bloodType:            [null as BloodType | null],
    allergies:            [''],
    chronicConditions:    [''],
    emergencyContactName: [''],
    emergencyContactPhone:['', vietnamPhoneValidator()],
    insuranceNumberPlain: [''],
    primaryDoctor:        [''],
    notes:                [''],
  });

  constructor() {
    effect(() => {
      const p = this.store.profile();
      if (p && !this.formPatched) {
        this.formPatched = true;
        this.form.patchValue({
          bloodType:             p.bloodType ?? null,
          allergies:             p.allergies ?? '',
          chronicConditions:     p.chronicConditions ?? '',
          emergencyContactName:  p.emergencyContactName ?? '',
          emergencyContactPhone: p.emergencyContactPhone ?? '',
          primaryDoctor:         p.primaryDoctor ?? '',
          notes:                 p.notes ?? '',
        });
      }
    });
  }

  ngOnInit(): void {
    this.store.loadProfile();
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const raw = this.form.getRawValue();
    try {
      await this.store.updateProfile({
        bloodType:             raw.bloodType ?? null,
        allergies:             raw.allergies || null,
        chronicConditions:     raw.chronicConditions || null,
        emergencyContactName:  raw.emergencyContactName || null,
        emergencyContactPhone: raw.emergencyContactPhone || null,
        insuranceNumberPlain:  raw.insuranceNumberPlain || null,
        primaryDoctor:         raw.primaryDoctor || null,
        notes:                 raw.notes || null,
      });
      this.router.navigate(['/health-records/profile']);
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel(): void {
    this.router.navigate(['/health-records/profile']);
  }
}

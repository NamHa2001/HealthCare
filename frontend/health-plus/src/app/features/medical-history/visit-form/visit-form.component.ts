import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MedicalHistoryStore } from '../medical-history.store';

@Component({
  selector: 'app-visit-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
  ],
  templateUrl: './visit-form.component.html',
})
export class VisitFormComponent implements OnInit {
  private readonly store = inject(MedicalHistoryStore);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly editId = signal<string | null>(null);

  protected readonly form = this.fb.group({
    visitDate:      [new Date().toISOString().slice(0, 10), Validators.required],
    facilityName:   ['', [Validators.required, Validators.maxLength(255)]],
    chiefComplaint: ['', Validators.required],
    diagnosis:      ['', Validators.required],
    doctorName:     [''],
    icd10Code:      ['', Validators.maxLength(10)],
    treatment:      [''],
    followUpDate:   [''],
    cost:           [null as number | null, Validators.min(0)],
    notes:          [''],
  });

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.loading.set(true);
      await this.store.loadVisitById(id);
      const v = this.store.currentVisit();
      if (v) {
        this.form.patchValue({
          visitDate: v.visitDate,
          facilityName: v.facilityName,
          chiefComplaint: v.chiefComplaint,
          diagnosis: v.diagnosis,
          doctorName: v.doctorName ?? '',
          icd10Code: v.icd10Code ?? '',
          treatment: v.treatment ?? '',
          followUpDate: v.followUpDate ?? '',
          cost: v.cost ?? null,
          notes: v.notes ?? '',
        });
      }
      this.loading.set(false);
    }
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const raw = this.form.getRawValue();
    const payload = {
      visitDate: raw.visitDate!,
      facilityName: raw.facilityName!,
      chiefComplaint: raw.chiefComplaint!,
      diagnosis: raw.diagnosis!,
      doctorName: raw.doctorName || null,
      icd10Code: raw.icd10Code || null,
      treatment: raw.treatment || null,
      followUpDate: raw.followUpDate || null,
      cost: raw.cost,
      notes: raw.notes || null,
    };

    this.saving.set(true);
    try {
      const id = this.editId();
      if (id) {
        await this.store.updateVisit(id, payload);
        this.router.navigate(['/medical-history/visits', id]);
      } else {
        const created = await this.store.createVisit(payload);
        this.router.navigate(['/medical-history/visits', created.id]);
      }
    } finally {
      this.saving.set(false);
    }
  }
}

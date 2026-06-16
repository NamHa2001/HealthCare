import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MedicationsStore } from '../medications.store';

@Component({
  selector: 'app-medication-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatButtonModule, MatCheckboxModule, MatFormFieldModule,
    MatIconModule, MatInputModule, MatProgressSpinnerModule, MatSelectModule,
  ],
  templateUrl: './medication-form.component.html',
})
export class MedicationFormComponent implements OnInit {
  private readonly store = inject(MedicationsStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly id = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly errorMsg = signal<string | null>(null);

  // Query params from OCR review
  private readonly ocrSourceDocId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    drugName: ['', Validators.required],
    strength: [''],
    dosageForm: [''],
    instructions: [''],
    startDate: [new Date().toISOString().substring(0, 10), Validators.required],
    endDate: [''],
    isOngoing: [false],
  });

  ngOnInit(): void {
    const paramId = this.route.snapshot.paramMap.get('id');
    this.id.set(paramId);

    // Pre-fill from query params (from OCR review)
    const qp = this.route.snapshot.queryParams;
    if (qp['drugName']) this.form.patchValue({ drugName: qp['drugName'] });
    if (qp['strength']) this.form.patchValue({ strength: qp['strength'] });
    if (qp['dosageForm']) this.form.patchValue({ dosageForm: qp['dosageForm'] });
    if (qp['instructions']) this.form.patchValue({ instructions: qp['instructions'] });
    if (qp['ocrDocId']) this.ocrSourceDocId.set(qp['ocrDocId']);

    if (paramId) {
      // Edit mode — find existing medication
      const existing = this.store.medications().find(m => m.id === paramId);
      if (existing) {
        this.form.patchValue({
          drugName: existing.drugName,
          strength: existing.strength ?? '',
          dosageForm: existing.dosageForm ?? '',
          instructions: existing.instructions ?? '',
          startDate: existing.startDate,
          endDate: existing.endDate ?? '',
          isOngoing: existing.isOngoing,
        });
      }
    }

    // If isOngoing checked, clear endDate
    this.form.controls.isOngoing.valueChanges.subscribe(v => {
      if (v) this.form.controls.endDate.setValue('');
    });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.errorMsg.set(null);

    const raw = this.form.getRawValue();
    const request = {
      drugName: raw.drugName,
      startDate: raw.startDate,
      isOngoing: raw.isOngoing,
      strength: raw.strength || null,
      dosageForm: raw.dosageForm || null,
      instructions: raw.instructions || null,
      endDate: raw.endDate || null,
      ocrSourceDocId: this.ocrSourceDocId(),
    };

    try {
      if (this.id()) {
        await this.store.updateMedication(this.id()!, request);
      } else {
        const med = await this.store.createMedication(request);
        // Navigate to schedule setup if new
        await this.router.navigate(['/medications', med.id, 'schedule']);
        return;
      }
      await this.router.navigate(['/medications']);
    } catch {
      this.errorMsg.set('Có lỗi xảy ra, vui lòng thử lại.');
    } finally {
      this.saving.set(false);
    }
  }
}

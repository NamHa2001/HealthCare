import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { VaccinesStore } from '../vaccines.store';

@Component({
  selector: 'app-vaccine-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatButtonModule, MatFormFieldModule, MatIconModule,
    MatInputModule, MatProgressSpinnerModule, MatSelectModule,
  ],
  templateUrl: './vaccine-form.component.html',
})
export class VaccineFormComponent implements OnInit {
  protected readonly store = inject(VaccinesStore);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly errorMsg = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    vaccineName: ['', Validators.required],
    doseNumber: [1, [Validators.required, Validators.min(1)]],
    injectionDate: [new Date().toISOString().substring(0, 10), Validators.required],
    vaccineCatalogId: [''],
    facility: [''],
    lotNumber: [''],
    administeredBy: [''],
    reaction: [''],
  });

  ngOnInit(): void {
    this.store.loadCatalog();

    this.form.controls.vaccineCatalogId.valueChanges.subscribe(catalogId => {
      if (catalogId) {
        const selected = this.store.catalog().find(c => c.id === catalogId);
        if (selected) {
          this.form.controls.vaccineName.setValue(selected.name);
        }
      }
    });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.errorMsg.set(null);

    const raw = this.form.getRawValue();

    try {
      await this.store.createRecord({
        vaccineName: raw.vaccineName,
        doseNumber: raw.doseNumber,
        injectionDate: raw.injectionDate,
        vaccineCatalogId: raw.vaccineCatalogId || null,
        facility: raw.facility || null,
        lotNumber: raw.lotNumber || null,
        administeredBy: raw.administeredBy || null,
        reaction: raw.reaction || null,
        documentId: null,
      });
      await this.router.navigate(['/vaccines']);
    } catch {
      this.errorMsg.set('Có lỗi xảy ra, vui lòng thử lại.');
    } finally {
      this.saving.set(false);
    }
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MedicationsStore } from '../../medications/medications.store';
import { ExtractedDrug, OcrResult } from '../../medications/models/medication.model';

@Component({
  selector: 'app-ocr-review',
  standalone: true,
  imports: [
    DecimalPipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './ocr-review.component.html',
})
export class OcrReviewComponent implements OnInit {
  private readonly store = inject(MedicationsStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly docId = signal<string>('');
  protected readonly result = signal<OcrResult | null>(null);
  protected readonly loading = signal(true);
  protected readonly errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    const docId = this.route.snapshot.paramMap.get('docId')!;
    this.docId.set(docId);

    // Check if OCR result already in store (just processed)
    const stored = this.store.ocrResult();
    if (stored && stored.documentId === docId) {
      this.result.set(stored);
      this.loading.set(false);
    } else {
      this.store.getOcrResult(docId)
        .then(r => {
          this.result.set(r);
          this.loading.set(false);
        })
        .catch(() => {
          this.errorMsg.set('Không thể tải kết quả OCR.');
          this.loading.set(false);
        });
    }
  }

  protected confidenceClass(score: number): string {
    if (score >= 0.75) return 'text-[#1cc88a]';
    if (score >= 0.5) return 'text-[#f6c23e]';
    return 'text-[#e74a3b]';
  }

  protected confidenceBg(score: number): string {
    if (score >= 0.75) return 'bg-[#1cc88a]/10 border-[#1cc88a]/30';
    if (score >= 0.5) return 'bg-[#f6c23e]/10 border-[#f6c23e]/30';
    return 'bg-[#e74a3b]/10 border-[#e74a3b]/30';
  }

  protected addMedication(drug: ExtractedDrug): void {
    this.router.navigate(['/medications/new'], {
      queryParams: {
        drugName: drug.drugName,
        strength: drug.strength ?? '',
        dosageForm: drug.dosageForm ?? '',
        instructions: [drug.dosage, drug.frequency, drug.instructions].filter(Boolean).join(', '),
        ocrDocId: this.docId(),
      },
    });
  }

  protected addAll(): void {
    const drugs = this.result()?.prescription?.drugs ?? [];
    if (drugs.length === 0) return;
    // Navigate to add first drug, user can navigate back to add more
    this.addMedication(drugs[0]);
  }
}

import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MedicalHistoryStore } from '../medical-history.store';
import { MedicalDocument } from '../models/medical-visit.model';

@Component({
  selector: 'app-document-viewer',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './document-viewer.component.html',
})
export class DocumentViewerComponent implements OnInit {
  private readonly store = inject(MedicalHistoryStore);
  private readonly sanitizer = inject(DomSanitizer);
  protected readonly dialogRef = inject(MatDialogRef<DocumentViewerComponent>);

  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly url = signal<string | null>(null);
  protected readonly safeUrl = signal<SafeResourceUrl | null>(null);

  constructor(@Inject(MAT_DIALOG_DATA) public readonly doc: MedicalDocument) {}

  protected get isImage(): boolean {
    return this.doc.mimeType.startsWith('image/');
  }

  protected get isPdf(): boolean {
    return this.doc.mimeType === 'application/pdf';
  }

  async ngOnInit(): Promise<void> {
    try {
      const url = await this.store.getDownloadUrl(this.doc.id);
      this.url.set(url);
      this.safeUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
    } catch {
      this.error.set(true);
    } finally {
      this.loading.set(false);
    }
  }

  protected download(): void {
    const url = this.url();
    if (url) window.open(url, '_blank');
  }
}

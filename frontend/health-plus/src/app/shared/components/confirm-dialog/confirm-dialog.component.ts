import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title class="text-base font-bold text-[#5a5c69]">{{ data.title }}</h2>
    <mat-dialog-content>
      <p class="text-sm text-[#858796]" [innerHTML]="data.message"></p>
    </mat-dialog-content>
    <mat-dialog-actions align="end" class="gap-2">
      <button mat-button (click)="ref.close(false)">{{ data.cancelLabel ?? 'Huỷ' }}</button>
      <button mat-flat-button
              [color]="data.danger ? 'warn' : 'primary'"
              (click)="ref.close(true)">
        {{ data.confirmLabel ?? 'Xác nhận' }}
      </button>
    </mat-dialog-actions>
  `,
})
export class ConfirmDialogComponent {
  readonly ref = inject(MatDialogRef<ConfirmDialogComponent>);
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
}

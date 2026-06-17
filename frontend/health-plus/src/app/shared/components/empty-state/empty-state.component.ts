import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="flex flex-col items-center justify-center py-14 text-[#858796]">
      <mat-icon class="text-gray-200 mb-3" style="font-size:3.5rem;width:3.5rem;height:3.5rem">
        {{ icon }}
      </mat-icon>
      <p class="text-sm font-medium mb-1">{{ title }}</p>
      @if (subtitle) {
        <p class="text-xs text-[#b7b9cc]">{{ subtitle }}</p>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  @Input() icon = 'inbox';
  @Input() title = 'Không có dữ liệu';
  @Input() subtitle = '';
}

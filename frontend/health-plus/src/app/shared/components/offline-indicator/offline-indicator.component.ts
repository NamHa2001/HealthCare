import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { OfflineService } from '../../../core/services/offline.service';

@Component({
  selector: 'app-offline-indicator',
  standalone: true,
  imports: [MatIconModule],
  template: `
    @if (!offline.isOnline()) {
      <div class="fixed bottom-4 left-1/2 -translate-x-1/2 z-50
                  flex items-center gap-2 px-4 py-2.5 rounded-full shadow-lg
                  bg-gray-800 text-white text-sm font-medium
                  transition-all duration-300">
        <mat-icon style="font-size:1rem;width:1rem;height:1rem">wifi_off</mat-icon>
        <span>Đang ngoại tuyến — các thay đổi sẽ được đồng bộ khi có kết nối</span>
      </div>
    }
  `,
})
export class OfflineIndicatorComponent {
  protected readonly offline = inject(OfflineService);
}

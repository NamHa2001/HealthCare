import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  @Input() isOpen = false;
  @Output() closeRequested = new EventEmitter<void>();

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Hồ sơ sức khỏe', icon: 'favorite', route: '/health-records' },
    { label: 'Lịch sử khám', icon: 'medical_services', route: '/medical-history' },
    { label: 'Vắc-xin', icon: 'vaccines', route: '/vaccines' },
    { label: 'Thuốc', icon: 'medication', route: '/medications' },
    { label: 'Quét đơn thuốc', icon: 'document_scanner', route: '/ocr' },
    { label: 'Nhắc lịch', icon: 'notifications', route: '/reminders' },
    { label: 'Phân tích', icon: 'bar_chart', route: '/analytics' },
    { label: 'Gia đình', icon: 'group', route: '/family' },
  ];

  onNavClick(): void {
    this.closeRequested.emit();
  }
}

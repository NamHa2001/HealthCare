import { Component, EventEmitter, Input, Output, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthStore } from '../../../core/auth/auth.store';

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

  private readonly authStore = inject(AuthStore);

  private readonly baseItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Hồ sơ sức khỏe', icon: 'favorite', route: '/health-records' },
    { label: 'Lịch sử khám', icon: 'medical_services', route: '/medical-history' },
    { label: 'Vắc-xin', icon: 'vaccines', route: '/vaccines' },
    { label: 'Thuốc', icon: 'medication', route: '/medications' },
    { label: 'Quét đơn thuốc', icon: 'document_scanner', route: '/ocr' },
    { label: 'Nhắc lịch', icon: 'notifications', route: '/reminders' },
    { label: 'Phân tích', icon: 'bar_chart', route: '/analytics' },
    { label: 'Chia sẻ hồ sơ', icon: 'share', route: '/sharing' },
    { label: 'Bác sĩ của tôi', icon: 'medical_information', route: '/doctor-links' },
    { label: 'Gia đình', icon: 'group', route: '/family' },
  ];

  /** Mục theo role: "Bệnh nhân của tôi" chỉ hiện với bác sĩ đã xác minh, "Quản trị" chỉ hiện với admin */
  readonly navItems = computed<NavItem[]>(() => [
    ...this.baseItems,
    ...(this.authStore.isDoctor()
      ? [{ label: 'Bệnh nhân của tôi', icon: 'groups', route: '/doctor/patients' }]
      : []),
    { label: 'Đăng ký bác sĩ', icon: 'badge', route: '/doctor-registration' },
    ...(this.authStore.isAdmin()
      ? [{ label: 'Quản trị', icon: 'admin_panel_settings', route: '/admin' }]
      : []),
  ]);

  onNavClick(): void {
    this.closeRequested.emit();
  }
}

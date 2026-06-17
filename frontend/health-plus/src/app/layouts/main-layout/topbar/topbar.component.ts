import { Component, EventEmitter, Output, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [MatButtonModule, MatDividerModule, MatIconModule, MatMenuModule, MatTooltipModule, RouterLink],
  templateUrl: './topbar.component.html',
})
export class TopbarComponent {
  @Output() menuToggled = new EventEmitter<void>();

  private readonly authStore = inject(AuthStore);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.authStore.currentUser;

  readonly userInitials = computed(() => {
    const user = this.authStore.currentUser();
    if (!user) return '?';
    const first = user.firstName?.[0] ?? '';
    const last = user.lastName?.[0] ?? '';
    return `${first}${last}`.toUpperCase() || '?';
  });

  readonly userFullName = computed(() => {
    const user = this.authStore.currentUser();
    if (!user) return '';
    return `${user.firstName} ${user.lastName}`.trim();
  });

  toggleMenu(): void {
    this.menuToggled.emit();
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/auth/login']),
      error: () => this.router.navigate(['/auth/login']),
    });
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { AdminStore } from '../admin.store';
import { AdminUser } from '../models/admin.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [FormsModule, DatePipe, MatIconModule, MatButtonModule, MatProgressSpinnerModule, EmptyStateComponent],
  templateUrl: './user-list.component.html',
})
export class UserListComponent implements OnInit {
  protected readonly store = inject(AdminStore);
  private readonly dialog = inject(MatDialog);

  searchTerm = signal('');
  filterActive = signal<boolean | undefined>(undefined);

  ngOnInit(): void {
    this.store.loadUsers();
  }

  onSearch(value: string): void {
    this.searchTerm.set(value);
    this.store.loadUsers(value || undefined, this.filterActive(), 1);
  }

  onFilterChange(value: string): void {
    const active = value === '' ? undefined : value === 'true';
    this.filterActive.set(active);
    this.store.loadUsers(this.searchTerm() || undefined, active, 1);
  }

  onPageChange(delta: number): void {
    const next = this.store.usersPage() + delta;
    this.store.loadUsers(this.searchTerm() || undefined, this.filterActive(), next);
  }

  async onToggle(user: AdminUser): Promise<void> {
    const action = user.isActive ? 'khoá' : 'mở khoá';
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: `${action.charAt(0).toUpperCase() + action.slice(1)} tài khoản`,
        message: `Bạn có chắc muốn ${action} tài khoản <strong>${user.email}</strong>?`,
        confirmLabel: action.charAt(0).toUpperCase() + action.slice(1),
        danger: user.isActive,
      },
      width: '400px',
    });
    const confirmed = await firstValueFrom(ref.afterClosed());
    if (confirmed) {
      await this.store.toggleUserActive(user.id);
    }
  }

  get totalPages(): number {
    const ps = 20;
    return Math.max(1, Math.ceil(this.store.usersTotal() / ps));
  }
}

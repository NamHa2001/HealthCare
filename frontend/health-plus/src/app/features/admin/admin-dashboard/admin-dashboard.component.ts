import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminStore } from '../admin.store';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [DatePipe, RouterLink, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './admin-dashboard.component.html',
})
export class AdminDashboardComponent implements OnInit {
  protected readonly store = inject(AdminStore);

  ngOnInit(): void {
    this.store.loadStats();
    this.store.loadUsers(undefined, undefined, 1);
    this.store.loadAuditLogs(undefined, 1);
  }
}

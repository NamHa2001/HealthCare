import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HealthRecordsStore } from '../health-records.store';
import { BLOOD_TYPE_LABELS } from '../models/health-profile.model';

@Component({
  selector: 'app-health-profile',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './health-profile.component.html',
})
export class HealthProfileComponent implements OnInit {
  protected readonly store = inject(HealthRecordsStore);
  protected readonly bloodTypeLabels = BLOOD_TYPE_LABELS;

  ngOnInit(): void {
    this.store.loadProfile();
  }
}

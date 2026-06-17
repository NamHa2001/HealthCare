import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VaccinesStore } from '../vaccines.store';

@Component({
  selector: 'app-vaccine-progress',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressBarModule,
    MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './vaccine-progress.component.html',
})
export class VaccineProgressComponent implements OnInit {
  protected readonly store = inject(VaccinesStore);

  ngOnInit(): void {
    this.store.loadProgress();
  }

  protected progressPercent(completed: number, total: number): number {
    return total > 0 ? completed / total : 0;
  }
}

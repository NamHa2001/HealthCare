import { Pipe, PipeTransform } from '@angular/core';

export interface BmiClass {
  label: string;
  badgeClass: string;
}

@Pipe({ name: 'bmiClass', standalone: true, pure: true })
export class BmiClassPipe implements PipeTransform {
  transform(bmi: number | null | undefined): BmiClass {
    if (bmi == null || bmi <= 0) return { label: '—', badgeClass: 'badge-neutral' };
    if (bmi < 18.5)  return { label: 'Thiếu cân',  badgeClass: 'badge-info' };
    if (bmi < 23.0)  return { label: 'Bình thường', badgeClass: 'badge-success' };
    if (bmi < 25.0)  return { label: 'Thừa cân',   badgeClass: 'badge-warning' };
    return                   { label: 'Béo phì',    badgeClass: 'badge-danger' };
  }
}

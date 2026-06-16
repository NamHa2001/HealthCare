import { Pipe, PipeTransform } from '@angular/core';

export interface BpClass {
  label: string;
  badgeClass: string;
}

@Pipe({ name: 'bpClass', standalone: true, pure: true })
export class BpClassPipe implements PipeTransform {
  transform(systolic: number | null | undefined, diastolic: number | null | undefined): BpClass {
    if (systolic == null || diastolic == null) return { label: '—', badgeClass: 'badge-neutral' };
    if (systolic >= 140 || diastolic >= 90) return { label: 'Cao huyết áp', badgeClass: 'badge-danger' };
    if (systolic >= 130 || diastolic >= 80) return { label: 'Tiền THA',     badgeClass: 'badge-warning' };
    if (systolic >= 120)                    return { label: 'Bình thường cao', badgeClass: 'badge-info' };
    return                                         { label: 'Bình thường',   badgeClass: 'badge-success' };
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { FamilyStore } from './family.store';
import { FamilyMember } from './models/family.model';

@Component({
  selector: 'app-family',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatDialogModule,
  ],
  templateUrl: './family.component.html',
})
export class FamilyComponent implements OnInit {
  protected readonly store  = inject(FamilyStore);
  private  readonly fb     = inject(FormBuilder);
  private  readonly snack  = inject(MatSnackBar);
  private  readonly api    = inject(ApiService);

  protected readonly savingGroup   = signal(false);
  protected readonly savingMember  = signal(false);
  protected readonly showAddForm   = signal(false);
  protected readonly showInvite    = signal(false);
  protected readonly sendingInvite = signal(false);
  protected readonly removingId    = signal<string | null>(null);

  readonly genders = [
    { value: 'male',   label: 'Nam' },
    { value: 'female', label: 'Nữ' },
    { value: 'other',  label: 'Khác' },
  ];

  readonly relationships = [
    { value: 'self',    label: 'Bản thân' },
    { value: 'spouse',  label: 'Vợ / Chồng' },
    { value: 'child',   label: 'Con' },
    { value: 'parent',  label: 'Cha / Mẹ' },
    { value: 'sibling', label: 'Anh / Chị / Em' },
  ];

  readonly groupForm = this.fb.group({
    name: ['Gia đình ' , [Validators.required, Validators.maxLength(100)]],
  });

  readonly inviteForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly memberForm = this.fb.group({
    fullName:     ['', [Validators.required, Validators.maxLength(255)]],
    dateOfBirth:  ['', Validators.required],
    gender:       ['male', Validators.required],
    relationship: ['child'],
  });

  async ngOnInit(): Promise<void> {
    await this.store.loadFamily();
  }

  protected async createGroup(): Promise<void> {
    this.groupForm.markAllAsTouched();
    if (this.groupForm.invalid || this.savingGroup()) return;
    this.savingGroup.set(true);
    try {
      await this.store.createGroup(this.groupForm.getRawValue().name!);
      this.snack.open('Đã tạo nhóm gia đình!', 'Đóng', { duration: 3000 });
    } catch {
      this.snack.open('Có lỗi xảy ra, vui lòng thử lại.', 'Đóng', { duration: 3000 });
    } finally {
      this.savingGroup.set(false);
    }
  }

  protected async addMember(): Promise<void> {
    this.memberForm.markAllAsTouched();
    if (this.memberForm.invalid || this.savingMember()) return;
    this.savingMember.set(true);
    const { fullName, dateOfBirth, gender, relationship } = this.memberForm.getRawValue();
    try {
      await this.store.addMember({
        fullName: fullName!,
        dateOfBirth: dateOfBirth!,
        gender: gender!,
        relationship: relationship || null,
      });
      this.memberForm.reset({ gender: 'male', relationship: 'child' });
      this.showAddForm.set(false);
      this.snack.open(`Đã thêm thành viên!`, 'Đóng', { duration: 3000 });
    } catch {
      this.snack.open('Có lỗi xảy ra, vui lòng thử lại.', 'Đóng', { duration: 3000 });
    } finally {
      this.savingMember.set(false);
    }
  }

  protected async removeMember(member: FamilyMember): Promise<void> {
    if (this.removingId()) return;
    this.removingId.set(member.id);
    try {
      await this.store.removeMember(member.id);
      this.snack.open(`Đã xóa ${member.fullName}`, 'Đóng', { duration: 3000 });
    } catch {
      this.snack.open('Có lỗi xảy ra, vui lòng thử lại.', 'Đóng', { duration: 3000 });
    } finally {
      this.removingId.set(null);
    }
  }

  protected async sendInvite(): Promise<void> {
    this.inviteForm.markAllAsTouched();
    if (this.inviteForm.invalid || this.sendingInvite()) return;
    this.sendingInvite.set(true);
    try {
      await firstValueFrom(this.api.post<void>('family/invite', this.inviteForm.getRawValue()));
      this.inviteForm.reset();
      this.showInvite.set(false);
      this.snack.open('Đã gửi lời mời qua email!', 'Đóng', { duration: 3000 });
    } catch (err: any) {
      const msg = err?.error?.error?.message ?? 'Có lỗi xảy ra, vui lòng thử lại.';
      this.snack.open(msg, 'Đóng', { duration: 4000 });
    } finally {
      this.sendingInvite.set(false);
    }
  }

  protected genderLabel(g: string): string {
    return this.genders.find(x => x.value === g)?.label ?? g;
  }

  protected relationshipLabel(r: string | null): string {
    if (!r) return '—';
    return this.relationships.find(x => x.value === r)?.label ?? r;
  }

  protected age(dob: string): number {
    const birth = new Date(dob);
    const now = new Date();
    let age = now.getFullYear() - birth.getFullYear();
    const m = now.getMonth() - birth.getMonth();
    if (m < 0 || (m === 0 && now.getDate() < birth.getDate())) age--;
    return age;
  }
}

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserProfile } from '../../models/auth';
import { StaffService } from '../staff.service';

@Component({
  selector: 'app-staff-list',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './staff-list.component.html',
  styleUrl: './staff-list.component.scss',
})
export class StaffListComponent implements OnInit {
  private readonly staffApi = inject(StaffService);
  private readonly translate = inject(TranslateService);

  readonly rows = signal<UserProfile[]>([]);
  readonly loadFailed = signal(false);

  ngOnInit(): void {
    this.refresh();
  }

  roleKey(role: string): string {
    return `staff.role.${role}`;
  }

  delete(id: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const ok =
      typeof globalThis.confirm === 'function'
        ? globalThis.confirm(this.translate.instant('staff.confirmDelete'))
        : true;
    if (!ok) {
      return;
    }

    this.staffApi.delete(id).subscribe({
      next: () => this.refresh(),
      error: () => this.loadFailed.set(true),
    });
  }

  refresh(): void {
    this.loadFailed.set(false);
    this.staffApi.list().subscribe({
      next: (data) => this.rows.set(data),
      error: () => {
        this.rows.set([]);
        this.loadFailed.set(true);
      },
    });
  }
}

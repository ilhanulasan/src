import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import {
  ALL_PERSONNEL_TYPES,
  DentalSpecialty,
  Personnel,
  PersonnelType,
} from '../../models/personnel';
import { PersonnelService } from '../personnel.service';

@Component({
  selector: 'app-personnel-list',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './personnel-list.component.html',
  styleUrl: './personnel-list.component.scss',
})
export class PersonnelListComponent implements OnInit {
  private readonly personnelApi = inject(PersonnelService);
  private readonly translate = inject(TranslateService);

  readonly rows = signal<Personnel[]>([]);
  readonly loadFailed = signal(false);
  readonly filterType = signal<PersonnelType | ''>('');
  readonly personnelTypes = ALL_PERSONNEL_TYPES;

  readonly filteredRows = computed(() => {
    const type = this.filterType();
    const list = this.rows();
    if (!type) {
      return list;
    }
    return list.filter((p) => p.personnelType === type);
  });

  ngOnInit(): void {
    this.refresh();
  }

  typeKey(type: PersonnelType): string {
    return `personnel.type.${type}`;
  }

  specialtyKey(specialty: DentalSpecialty): string {
    return `personnel.specialty.${specialty}`;
  }

  onFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as PersonnelType | '';
    this.filterType.set(value);
  }

  delete(id: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const ok =
      typeof globalThis.confirm === 'function'
        ? globalThis.confirm(this.translate.instant('personnel.confirmDelete'))
        : true;
    if (!ok) {
      return;
    }

    this.personnelApi.delete(id).subscribe({
      next: () => this.refresh(),
      error: () => this.loadFailed.set(true),
    });
  }

  refresh(): void {
    this.loadFailed.set(false);
    this.personnelApi.list().subscribe({
      next: (data) => this.rows.set(data),
      error: () => {
        this.rows.set([]);
        this.loadFailed.set(true);
      },
    });
  }
}

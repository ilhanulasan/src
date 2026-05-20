import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { finalize, filter, map } from 'rxjs';

import { PatientService } from '../../patients/patient.service';
import {
  OdontographDocument,
  OdontographEngine,
  OdontographEngineDamage,
} from '../models/odontograph';
import {
  ODONTOGRAPH_CANVAS_BASE,
  ODONTOGRAPH_CANVAS_SCALE,
  ODONTOGRAPH_LAYOUT,
  ODONTOGRAPH_TOOL_GROUPS,
} from '../odontograph-catalog';
import { installOdontographI18n } from '../odontograph-i18n';
import { OdontographScriptLoader } from '../odontograph-script.loader';
import { OdontographService } from '../odontograph.service';

@Component({
  selector: 'app-odontograph-editor',
  standalone: true,
  imports: [CommonModule, TranslatePipe, RouterLink],
  templateUrl: './odontograph-editor.component.html',
  styleUrl: './odontograph-editor.component.scss',
})
export class OdontographEditorComponent implements OnInit, AfterViewInit {
  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly translate = inject(TranslateService);
  private readonly scriptLoader = inject(OdontographScriptLoader);
  private readonly odontographApi = inject(OdontographService);
  private readonly patientsApi = inject(PatientService);

  private engine: OdontographEngine | null = null;
  private pendingDocument: OdontographDocument | null = null;
  private viewReady = false;

  chartBusy = signal(true);
  saveBusy = signal(false);
  chartType = signal<'adult' | 'child'>('adult');
  patientRouteId = signal('');
  patientName = signal('');
  observations = signal('');
  specifications = signal('');

  alert = signal<{ level: 'ok' | 'err'; key: string } | null>(null);
  selectedDamageId = signal<number | null>(null);

  readonly toolGroups = ODONTOGRAPH_TOOL_GROUPS;
  readonly canvasPixelWidth = Math.round(ODONTOGRAPH_CANVAS_BASE.width * ODONTOGRAPH_CANVAS_SCALE);
  readonly canvasPixelHeight = Math.round(ODONTOGRAPH_CANVAS_BASE.height * ODONTOGRAPH_CANVAS_SCALE);

  private langTick = signal(0);

  ngOnInit(): void {
    installOdontographI18n(this.translate);

    this.translate.onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.langTick.update((n) => n + 1));

    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map((pm) => pm.get('id')),
        filter((id): id is string => !!id),
      )
      .subscribe((id) => this.bootstrap(id));
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    this.tryInitEngine();
  }

  private bootstrap(patientId: string): void {
    this.patientRouteId.set(patientId);
    this.chartBusy.set(true);
    this.alert.set(null);

    this.patientsApi.get(patientId).subscribe({
      next: (p) => this.patientName.set(`${p.name} ${p.surname}`.trim()),
      error: () => this.patientName.set(''),
    });

    this.scriptLoader.ensureLoaded().then(
      () => {
        this.odontographApi.getForPatient(patientId).subscribe({
          next: (doc) => {
            this.pendingDocument = doc ?? this.emptyDocument();
            if (!doc) {
              this.patientsApi.get(patientId).subscribe({
                next: (patient) => {
                  const inferred = this.chartTypeFromDob(patient.dateOfBirth);
                  this.chartType.set(inferred);
                  this.pendingDocument = { ...this.emptyDocument(), type: inferred };
                  this.tryInitEngine();
                },
                error: () => this.tryInitEngine(),
              });
            } else {
              this.chartType.set(doc.type === 'child' ? 'child' : 'adult');
              this.observations.set(doc.observations ?? '');
              this.specifications.set(doc.specifications ?? '');
              this.tryInitEngine();
            }
          },
          error: () => {
            this.alert.set({ level: 'err', key: 'odontograph.loadChartError' });
            this.chartBusy.set(false);
          },
        });
      },
      () => {
        this.alert.set({ level: 'err', key: 'odontograph.scriptLoadError' });
        this.chartBusy.set(false);
      },
    );
  }

  private tryInitEngine(): void {
    if (!this.viewReady || !this.pendingDocument || this.engine) {
      return;
    }

    installOdontographI18n(this.translate);
    window.OdontographUseHtmlToolbar = true;
    window.OdontographLayout = ODONTOGRAPH_LAYOUT;

    const canvas = this.canvasRef.nativeElement;
    canvas.width = Math.round(ODONTOGRAPH_CANVAS_BASE.width * ODONTOGRAPH_CANVAS_SCALE);
    canvas.height = Math.round(ODONTOGRAPH_CANVAS_BASE.height * ODONTOGRAPH_CANVAS_SCALE);

    const engine = new window.Engine();
    engine.setCanvas(canvas);
    engine.init();

    canvas.addEventListener('mousedown', (e) => engine.onMouseClick(e), false);
    canvas.addEventListener('mousemove', (e) => engine.onMouseMove(e), false);
    window.addEventListener('keydown', (e) => engine.onButtonClick(e), false);

    this.engine = engine;
    this.applyPatientHeader();
    this.loadDocumentIntoEngine(this.pendingDocument);
    this.pendingDocument = null;
    this.chartBusy.set(false);
  }

  private applyPatientHeader(): void {
    const engine = this.engine;
    if (!engine) return;

    const today = new Date().toLocaleDateString();
    engine.loadPatientData(
      '',
      this.patientName() || '—',
      this.patientRouteId(),
      '',
      today,
      '',
      this.observations(),
      this.specifications(),
    );
  }

  private loadDocumentIntoEngine(doc: OdontographDocument): void {
    const engine = this.engine;
    if (!engine) return;

    engine.reset();
    this.selectedDamageId.set(null);
    engine.setDamage(0);

    const adultRows = doc.damages.filter((r) => this.isAdultChartTooth(r.tooth));
    const childRows = doc.damages.filter((r) => this.isChildChartTooth(r.tooth));

    engine.changeView('0');
    for (const row of adultRows) {
      this.loadEngineRow(engine, row);
    }

    engine.changeView('1');
    for (const row of childRows) {
      this.loadEngineRow(engine, row);
    }

    engine.changeView(doc.type === 'child' ? '1' : '0');
    this.chartType.set(doc.type === 'child' ? 'child' : 'adult');

    engine.observations = doc.observations ?? '';
    engine.specifications = doc.specifications ?? '';
    this.observations.set(engine.observations);
    this.specifications.set(engine.specifications);
    this.applyPatientHeader();
    engine.update();
  }

  /** Permanent dentition + inter-tooth spaces (engine uses ids >= 1000 for spaces). */
  private isAdultChartTooth(tooth: number): boolean {
    if (tooth >= 1000) {
      return true;
    }
    const quadrant = Math.floor(tooth / 10);
    return quadrant >= 1 && quadrant <= 4;
  }

  private isChildChartTooth(tooth: number): boolean {
    const quadrant = Math.floor(tooth / 10);
    return quadrant >= 5 && quadrant <= 8;
  }

  private loadEngineRow(engine: OdontographEngine, row: OdontographDocument['damages'][number]): void {
    const damageRaw = row.damage === '' ? 0 : Number(row.damage) || row.damage;
    engine.load(row.tooth, damageRaw, row.surface ?? '0', row.note ?? '');
  }

  save(): void {
    const engine = this.engine;
    const patientId = this.patientRouteId();
    if (!engine || !patientId) {
      this.alert.set({ level: 'err', key: 'odontograph.missingPatient' });
      return;
    }

    const body: OdontographDocument = {
      type: engine.adultShowing ? 'adult' : 'child',
      damages: this.mapEngineData(engine.getData()),
      observations: this.observations(),
      specifications: this.specifications(),
    };

    this.saveBusy.set(true);
    this.odontographApi
      .save(patientId, body)
      .pipe(finalize(() => this.saveBusy.set(false)))
      .subscribe({
        next: (saved) => {
          this.chartType.set(saved.type === 'child' ? 'child' : 'adult');
          this.observations.set(saved.observations ?? '');
          this.specifications.set(saved.specifications ?? '');
          this.alert.set({ level: 'ok', key: 'odontograph.saved' });
        },
        error: () => this.alert.set({ level: 'err', key: 'odontograph.saveError' }),
      });
  }

  private mapEngineData(rows: OdontographEngineDamage[]) {
    return rows.map((r) => ({
      tooth: Number(r.tooth),
      damage: r.damage === undefined || r.damage === null ? '' : String(r.damage),
      surface: r.surface ?? '0',
      note: r.note ?? '',
    }));
  }

  onObservationsInput(value: string): void {
    this.observations.set(value);
    if (this.engine) {
      this.engine.observations = value;
      this.applyPatientHeader();
    }
  }

  onSpecificationsInput(value: string): void {
    this.specifications.set(value);
    if (this.engine) {
      this.engine.specifications = value;
      this.applyPatientHeader();
    }
  }

  dismissAlert(): void {
    this.alert.set(null);
  }

  damageLabel(id: number): string {
    void this.langTick();
    const key = `odontograph.damage.${id}`;
    const value = this.translate.instant(key);
    return value !== key ? value : key;
  }

  selectDamage(id: number): void {
    const engine = this.engine;
    if (!engine) return;

    if (this.selectedDamageId() === id) {
      this.selectedDamageId.set(null);
      engine.setDamage(0);
    } else {
      this.selectedDamageId.set(id);
      engine.setDamage(id);
    }
    engine.update();
  }

  setAdultView(): void {
    const engine = this.engine;
    if (!engine) return;
    engine.changeView('0');
    engine.adult.active = true;
    engine.child.active = false;
    this.chartType.set('adult');
    engine.update();
  }

  setChildView(): void {
    const engine = this.engine;
    if (!engine) return;
    engine.changeView('1');
    engine.adult.active = false;
    engine.child.active = true;
    this.chartType.set('child');
    engine.update();
  }

  resetChart(): void {
    const engine = this.engine;
    if (!engine) return;
    engine.reset();
    this.selectedDamageId.set(null);
    engine.setDamage(0);
    engine.update();
  }

  private emptyDocument(): OdontographDocument {
    return { type: 'adult', damages: [] };
  }

  private chartTypeFromDob(raw?: string | null): 'adult' | 'child' {
    if (!raw) return 'adult';
    const birth = new Date(raw);
    if (Number.isNaN(birth.getTime())) return 'adult';

    const today = new Date();
    let age = today.getFullYear() - birth.getFullYear();
    const m = today.getMonth() - birth.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) {
      age--;
    }
    return age < 12 ? 'child' : 'adult';
  }
}

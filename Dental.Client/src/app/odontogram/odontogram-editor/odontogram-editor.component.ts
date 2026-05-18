import { CommonModule } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { finalize, filter, map } from 'rxjs';
import { PatientService } from '../../patients/patient.service';
import {
  Odontogram,
  Pathology,
  ToothTreatment,
  BridgeTreatment,
  Treatment,
  ToothPathology,
} from '../models/odontogram';
import { ToothComponent } from '../tooth/tooth.component';
import { OdontogramService } from '../odontogram.service';
import { clinicTreatments, orderedPathologies, treatmentById } from '../dental-catalog';

@Component({
  selector: 'app-odontogram-editor',
  standalone: true,
  imports: [CommonModule, ToothComponent, TranslatePipe, RouterLink],
  templateUrl: './odontogram-editor.component.html',
  styleUrl: './odontogram-editor.component.scss',
})
export class OdontogramEditorComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly odontogramApi = inject(OdontogramService);
  private readonly patientsApi = inject(PatientService);

  private readonly ADULT_UPPER_TEETH = [
    18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28,
  ];
  private readonly ADULT_LOWER_TEETH = [
    48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38,
  ];
  private readonly CHILD_UPPER_TEETH = [55, 54, 53, 52, 51, 61, 62, 63, 64, 65];
  private readonly CHILD_LOWER_TEETH = [85, 84, 83, 82, 81, 71, 72, 73, 74, 75];
  private readonly WHOLE_TOOTH_FACE = 0;

  pathologies = signal<Pathology[]>([]);
  treatments = signal(clinicTreatments());

  selectedPathology = signal<Pathology | null>(null);
  selectedTreatment = signal<Treatment | null>(null);
  selectedTreatmentStatus = signal<'pending' | 'done'>('pending');

  odontogram = signal<Odontogram | null>(null);
  chartBusy = signal(false);
  saveBusy = signal(false);

  odontogramType = signal<'adult' | 'child'>('adult');
  activeTab = signal<'pathologies' | 'treatments'>('pathologies');
  isToggleAbsenceMode = signal(false);
  isBridgeMode = signal(false);
  bridgeFirstPilar = signal<number | null>(null);
  isDeleteMode = signal(false);

  alert = signal<{ level: 'ok' | 'err'; key: string } | null>(null);
  readonly patientRouteId = signal<string>('');

  upperRightTeeth = computed(() =>
    this.odontogramType() === 'child'
      ? this.CHILD_UPPER_TEETH.slice(0, 5)
      : this.ADULT_UPPER_TEETH.slice(0, 8),
  );
  upperLeftTeeth = computed(() =>
    this.odontogramType() === 'child'
      ? this.CHILD_UPPER_TEETH.slice(5)
      : this.ADULT_UPPER_TEETH.slice(8),
  );
  lowerRightTeeth = computed(() =>
    this.odontogramType() === 'child'
      ? this.CHILD_LOWER_TEETH.slice(0, 5)
      : this.ADULT_LOWER_TEETH.slice(0, 8),
  );
  lowerLeftTeeth = computed(() =>
    this.odontogramType() === 'child'
      ? this.CHILD_LOWER_TEETH.slice(5)
      : this.ADULT_LOWER_TEETH.slice(8),
  );

  ngOnInit(): void {
    this.pathologies.set(orderedPathologies());

    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map((pm) => pm.get('id')),
        filter((id): id is string => !!id),
      )
      .subscribe((id) => this.loadCharts(id));
  }

  pathologyLabel(id: number): string {
    return `odontogram.pathology.${id}`;
  }

  treatmentLabel(id: number): string {
    return `odontogram.treatment.${id}`;
  }

  private loadCharts(patientId: string): void {
    this.patientRouteId.set(patientId);
    this.odontogram.set(null);
    this.chartBusy.set(true);
    this.alert.set(null);

    this.odontogramApi.getForPatient(patientId).subscribe({
      next: (doc) => {
        if (doc) {
          const cleaned = this.cleanOdontogramStructure(doc);
          this.odontogram.set(this.finalizeChart(cleaned));
          const t =
            cleaned.type === 'child' ? ('child' as const) : ('adult' as const);
          this.odontogramType.set(t);
          this.chartBusy.set(false);
          return;
        }

        this.patientsApi.get(patientId).subscribe({
          next: (patient) => {
            const inferred = this.chartTypeFromDob(patient.dateOfBirth);
            this.odontogramType.set(inferred);
            this.odontogram.set({
              patient: { id: patientId },
              type: inferred,
              toothPathologies: [],
              toothTreatments: [],
              bridgeTreatments: [],
            });
            this.chartBusy.set(false);
          },
          error: () => {
            this.odontogramType.set('adult');
            this.odontogram.set({
              patient: { id: patientId },
              type: 'adult',
              toothPathologies: [],
              toothTreatments: [],
              bridgeTreatments: [],
            });
            this.chartBusy.set(false);
          },
        });
      },
      error: () => {
        this.alert.set({
          level: 'err',
          key: 'odontogram.loadChartError',
        });
        this.chartBusy.set(false);
      },
    });
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

  private cleanOdontogramStructure(chart: Odontogram): Odontogram {
    let toothTreatments = chart.toothTreatments || [];
    if (!Array.isArray(toothTreatments)) {
      toothTreatments = Object.values(
        toothTreatments as Record<string, ToothTreatment>,
      );
    }

    let bridgeTreatments = chart.bridgeTreatments || [];
    if (!Array.isArray(bridgeTreatments)) {
      bridgeTreatments = Object.values(
        bridgeTreatments as Record<string, BridgeTreatment>,
      );
    }

    const cleanedTreats = toothTreatments.filter((tt) => tt.treatment && !Array.isArray(tt.treatment));
    const cleanedBridges = bridgeTreatments.filter((bt) => bt.treatment && !Array.isArray(bt.treatment));

    return {
      ...chart,
      toothTreatments: cleanedTreats,
      bridgeTreatments: cleanedBridges,
    };
  }

  private finalizeChart(chart: Odontogram): Odontogram {
    const cat = orderedPathologies();
    const pathologyMap = new Map(cat.map((p) => [p.id, p]));

    const toothPathologies = this.normalizeToothPathologies(chart.toothPathologies).map((item) => ({
      ...item,
      pathology: {
        ...item.pathology,
        color: pathologyMap.get(item.pathology.id)?.color ?? item.pathology.color,
      },
    }));

    const toothTreatments = (chart.toothTreatments || []).map((tt) => ({
      ...tt,
      treatment: this.hydrateTreatment(tt.treatment),
    }));

    const bridgeTreatments = (chart.bridgeTreatments || []).map((bt) => ({
      ...bt,
      treatment: this.hydrateTreatment(bt.treatment),
    }));

    return { ...chart, toothPathologies, toothTreatments, bridgeTreatments };
  }

  private hydrateTreatment(raw: Treatment): Treatment {
    const merged = treatmentById(raw.id);
    return merged ?? { id: raw.id, kind: 'endo' };
  }

  private normalizeToothPathologies(value: unknown): ToothPathology[] {
    if (Array.isArray(value)) return value as ToothPathology[];
    if (value && typeof value === 'object') {
      return Object.values(value as Record<string, ToothPathology>);
    }
    return [];
  }

  handleFaceClick(toothNumber: number, face: number): void {
    if (this.isDeleteMode()) {
      this.handleDeleteFaceClick(toothNumber, face);
      return;
    }

    if (this.activeTab() === 'treatments') {
      this.handleTreatmentFaceClick(toothNumber, face);
      return;
    }

    if (this.isToggleAbsenceMode()) {
      this.toggleAbsenceNatural(toothNumber);
      return;
    }

    const activeTool = this.selectedPathology();
    if (!activeTool) return;

    this.odontogram.update((prev) => {
      if (!prev) return null;

      const pathologies = [...prev.toothPathologies];
      const existingIdx = pathologies.findIndex(
        (p) => p.tooth.toothNumber === toothNumber && p.toothFace === face,
      );

      if (existingIdx > -1) {
        if (pathologies[existingIdx].pathology.id === activeTool.id) {
          pathologies.splice(existingIdx, 1);
        } else {
          pathologies[existingIdx] = { ...pathologies[existingIdx], pathology: activeTool };
        }
      } else {
        pathologies.push({
          tooth: { id: 0, toothNumber },
          pathology: activeTool,
          toothFace: face,
        });
      }

      return { ...prev, toothPathologies: pathologies };
    });
  }

  getPatosForTooth(num: number): ToothPathology[] {
    return this.odontogram()?.toothPathologies.filter((p) => p.tooth.toothNumber === num) ?? [];
  }

  save(): void {
    const current = this.odontogram();
    if (!current) return;

    const pid = String(this.extractPatientId(current));
    if (!pid) {
      this.alert.set({ level: 'err', key: 'odontogram.missingPatient' });
      return;
    }

    let toothTreatmentsArray = current.toothTreatments || [];
    let bridgeTreatmentsArray = current.bridgeTreatments || [];
    if (!Array.isArray(toothTreatmentsArray)) {
      toothTreatmentsArray = Object.values(toothTreatmentsArray);
    }
    if (!Array.isArray(bridgeTreatmentsArray)) {
      bridgeTreatmentsArray = Object.values(bridgeTreatmentsArray);
    }

    const processedToothTreatments = toothTreatmentsArray.flatMap((tt) => {
      if (!tt.treatment || Array.isArray(tt.treatment)) return [];
      const treatmentId = tt.treatment.id;
      if (!treatmentId) return [];
      return [
        {
          treatment: { id: treatmentId },
          toothNumber: tt.toothNumber,
          toothFace: tt.toothFace,
          status: tt.status,
        },
      ];
    });

    const processedBridgeTreatments = bridgeTreatmentsArray.flatMap((bt) => {
      if (!bt.treatment || Array.isArray(bt.treatment)) return [];
      const treatmentId = bt.treatment.id;
      if (!treatmentId) return [];
      return [
        {
          treatment: { id: treatmentId },
          startTooth: bt.startTooth,
          endTooth: bt.endTooth,
          status: bt.status,
        },
      ];
    });

    const body = {
      id: current.id,
      type: current.type ?? this.odontogramType(),
      patient: { id: pid },
      toothPathologies: current.toothPathologies.map((tp) => ({
        tooth: { id: tp.tooth.id ?? 0, toothNumber: tp.tooth.toothNumber },
        pathology: { id: tp.pathology.id },
        toothFace: tp.toothFace,
      })),
      toothTreatments: processedToothTreatments,
      bridgeTreatments: processedBridgeTreatments,
    };

    this.saveBusy.set(true);
    this.odontogramApi
      .save(pid, body as Odontogram)
      .pipe(finalize(() => this.saveBusy.set(false)))
      .subscribe({
        next: (saved) => {
          const cleaned = this.cleanOdontogramStructure(saved);
          this.odontogram.set(this.finalizeChart(cleaned));
          const t =
            cleaned.type === 'child' ? ('child' as const) : ('adult' as const);
          this.odontogramType.set(t);
          this.alert.set({ level: 'ok', key: 'odontogram.saved' });
        },
        error: () => {
          this.alert.set({ level: 'err', key: 'odontogram.saveError' });
        },
      });
  }

  private extractPatientId(chart: Odontogram): string {
    const p = chart.patient;
    if (typeof p === 'string') return p;
    return p?.id ?? '';
  }

  onTabChange(tab: 'pathologies' | 'treatments'): void {
    this.activeTab.set(tab);
  }

  toggleAbsenceNatural(toothNumber: number): void {
    const absencePathology = this.pathologies().find((p) => p.id === 4);
    if (!absencePathology) return;

    this.odontogram.update((prev) => {
      if (!prev) return null;

      const pathologies = [...prev.toothPathologies];
      const existingIdx = pathologies.findIndex(
        (p) =>
          p.tooth.toothNumber === toothNumber &&
          p.toothFace === this.WHOLE_TOOTH_FACE &&
          p.pathology.id === 4,
      );

      if (existingIdx > -1) pathologies.splice(existingIdx, 1);
      else {
        pathologies.push({
          tooth: { id: 0, toothNumber },
          pathology: absencePathology,
          toothFace: this.WHOLE_TOOTH_FACE,
        });
      }

      return { ...prev, toothPathologies: pathologies };
    });
  }

  selectPathology(pathology: Pathology): void {
    this.selectedPathology.set(pathology);
    this.isToggleAbsenceMode.set(pathology.id === 4);
  }

  selectTreatment(treatment: Treatment): void {
    this.selectedTreatment.set(treatment);
    this.isToggleAbsenceMode.set(false);
    this.isDeleteMode.set(false);

    const isBridge = treatment.kind === 'bridge';
    this.isBridgeMode.set(isBridge);
    if (isBridge) {
      this.bridgeFirstPilar.set(null);
      this.alert.set({ level: 'ok', key: 'odontogram.bridgeActivated' });
    }
  }

  toggleDeleteMode(): void {
    this.isDeleteMode.update((v) => !v);
    if (!this.isDeleteMode()) return;
    this.isBridgeMode.set(false);
    this.isToggleAbsenceMode.set(false);
  }

  handleTreatmentFaceClick(toothNumber: number, face: number): void {
    const activeTreatment = this.selectedTreatment();
    const treatmentStatus = this.selectedTreatmentStatus();
    if (!activeTreatment) return;

    if (activeTreatment.kind === 'bridge') {
      this.handleBridgeSelection(toothNumber, treatmentStatus);
      return;
    }

    const targetFace = activeTreatment.kind === 'crown' ? this.WHOLE_TOOTH_FACE : face;

    this.odontogram.update((prev) => {
      if (!prev) return null;

      let arr = prev.toothTreatments || [];
      if (!Array.isArray(arr)) arr = Object.values(arr);
      const treatments: ToothTreatment[] = [...arr];
      const activeTreatmentId = activeTreatment.id;
      let existingIdx = -1;
      for (let i = 0; i < treatments.length; i++) {
        const t = treatments[i];
        const tid = t.treatment.id;
        if (t.toothNumber === toothNumber && t.toothFace === targetFace && tid === activeTreatmentId) {
          existingIdx = i;
          break;
        }
      }

      if (existingIdx > -1) treatments.splice(existingIdx, 1);
      else {
        treatments.push({
          treatment: activeTreatment,
          toothNumber,
          toothFace: targetFace,
          status: treatmentStatus,
        });
      }

      return { ...prev, toothTreatments: treatments };
    });
  }

  getTreatmentsForTooth(toothNumber: number): ToothTreatment[] {
    const all = this.odontogram()?.toothTreatments;
    if (!all) return [];
    const treatmentsArray = (Array.isArray(all) ? all : Object.values(all)) as ToothTreatment[];
    return treatmentsArray.filter((t) => {
      const k = t.treatment.kind ?? 'endo';
      return t.toothNumber === toothNumber && k !== 'bridge';
    });
  }

  handleDeleteFaceClick(toothNumber: number, face: number): void {
    this.odontogram.update((prev) => {
      if (!prev) return prev;

      if (this.activeTab() === 'pathologies') {
        const pathologies = [...prev.toothPathologies];
        const idx = pathologies.findIndex(
          (p) => p.tooth.toothNumber === toothNumber && p.toothFace === face,
        );
        if (idx > -1) {
          pathologies.splice(idx, 1);
          return { ...prev, toothPathologies: pathologies };
        }
      } else {
        let arr = prev.toothTreatments || [];
        if (!Array.isArray(arr)) arr = Object.values(arr);
        const treatments = [...arr];
        const idx = treatments.findIndex((t) => t.toothNumber === toothNumber && t.toothFace === face);
        if (idx > -1) {
          treatments.splice(idx, 1);
          return { ...prev, toothTreatments: treatments };
        }
      }

      return prev;
    });
  }

  getBridgesForTooth(toothNumber: number): BridgeTreatment[] {
    const all = this.odontogram()?.bridgeTreatments;
    if (!all) return [];
    const bridgesArray = (Array.isArray(all) ? all : Object.values(all)) as BridgeTreatment[];
    return bridgesArray.filter((bridge) => {
      const min = Math.min(bridge.startTooth, bridge.endTooth);
      const max = Math.max(bridge.startTooth, bridge.endTooth);
      return toothNumber >= min && toothNumber <= max;
    });
  }

  private areSameQuadrant(tooth1: number, tooth2: number): boolean {
    const getQuadrant = (tooth: number) => Math.floor(tooth / 10);
    return getQuadrant(tooth1) === getQuadrant(tooth2);
  }

  private handleBridgeSelection(toothNumber: number, status: 'pending' | 'done'): void {
    const firstPilar = this.bridgeFirstPilar();
    const activeTreatment = this.selectedTreatment();

    if (!activeTreatment?.kind || activeTreatment.kind !== 'bridge') return;

    if (!firstPilar) {
      this.bridgeFirstPilar.set(toothNumber);
      this.alert.set({ level: 'ok', key: 'odontogram.bridgePickSecond' });
      return;
    }

    if (firstPilar === toothNumber) {
      this.alert.set({ level: 'ok', key: 'odontogram.bridgeSameTooth' });
      return;
    }

    if (!this.areSameQuadrant(firstPilar, toothNumber)) {
      this.alert.set({ level: 'err', key: 'odontogram.bridgeWrongQuadrant' });
      return;
    }

    this.completeBridgeSelection(firstPilar, toothNumber, status);
    this.bridgeFirstPilar.set(null);
  }

  private completeBridgeSelection(
    startTooth: number,
    endTooth: number,
    status: 'pending' | 'done',
  ): void {
    const activeTreatment = this.selectedTreatment();
    if (!activeTreatment?.kind || activeTreatment.kind !== 'bridge') return;

    this.odontogram.update((prev) => {
      if (!prev) return null;

      let arr = prev.bridgeTreatments || [];
      if (!Array.isArray(arr)) arr = Object.values(arr);
      const bridges: BridgeTreatment[] = [...arr];
      const minTooth = Math.min(startTooth, endTooth);
      const maxTooth = Math.max(startTooth, endTooth);

      const existingIdx = bridges.findIndex(
        (b) =>
          b.treatment.id === activeTreatment.id &&
          ((b.startTooth === minTooth && b.endTooth === maxTooth) ||
            (b.startTooth === maxTooth && b.endTooth === minTooth)),
      );

      if (existingIdx > -1) {
        bridges.splice(existingIdx, 1);
      } else {
        bridges.push({
          treatment: activeTreatment,
          startTooth: minTooth,
          endTooth: maxTooth,
          status,
        });
      }

      return { ...prev, bridgeTreatments: bridges };
    });
  }

  dismissAlert(): void {
    this.alert.set(null);
  }
}

import { Component, input, output } from '@angular/core';
import { ToothPathology, ToothTreatment, BridgeTreatment } from '../models/odontogram';

@Component({
  selector: 'app-tooth',
  imports: [],
  templateUrl: './tooth.component.html',
  styleUrl: './tooth.component.scss',
})
export class ToothComponent {
  private readonly WHOLE_TOOTH_FACE = 0;

  toothNumber = input.required<number>();
  appliedPathologies = input<ToothPathology[]>([]);
  appliedTreatments = input<ToothTreatment[]>([]);
  appliedBridges = input<BridgeTreatment[]>([]);
  activeTab = input<'pathologies' | 'treatments'>('pathologies');

  onFaceClick = output<number>();

  getFaceColor(face: number): string {
    const pathologies = this.appliedPathologies();
    const wholeTooth = pathologies.find((p) => p.toothFace === this.WHOLE_TOOTH_FACE);

    if (wholeTooth) {
      return wholeTooth.pathology.color || '#ff0000';
    }

    const found = pathologies.find((p) => p.toothFace === face);
    return found ? found.pathology.color || '#ff0000' : '#ffffff';
  }

  hasPathologyId4(): boolean {
    return this.appliedPathologies().some((p) => p.pathology.id === 4);
  }

  hasTreatments(): boolean {
    return this.appliedTreatments().length > 0;
  }

  /** Bridge spans use treatment.kind === bridge */
  isPillarTooth(): boolean {
    const bridges = this.appliedBridges();
    const tooth = this.toothNumber();

    return bridges.some((b) => {
      const k = b.treatment.kind;
      return (
        k === 'bridge' &&
        (tooth === Math.min(b.startTooth, b.endTooth) || tooth === Math.max(b.startTooth, b.endTooth))
      );
    });
  }

  isLeftPillarTooth(): boolean {
    const bridges = this.appliedBridges();
    const tooth = this.toothNumber();

    return bridges.some((b) => {
      const k = b.treatment.kind;
      return k === 'bridge' && tooth === Math.min(b.startTooth, b.endTooth);
    });
  }

  isRightPillarTooth(): boolean {
    const bridges = this.appliedBridges();
    const tooth = this.toothNumber();

    return bridges.some((b) => {
      const k = b.treatment.kind;
      return k === 'bridge' && tooth === Math.max(b.startTooth, b.endTooth);
    });
  }

  isIntermediateTooth(): boolean {
    const bridges = this.appliedBridges();
    const tooth = this.toothNumber();

    return bridges.some((b) => {
      const k = b.treatment.kind;
      if (k !== 'bridge') return false;
      const min = Math.min(b.startTooth, b.endTooth);
      const max = Math.max(b.startTooth, b.endTooth);
      return tooth > min && tooth < max;
    });
  }

  getBridgesIncludingTooth(): BridgeTreatment[] {
    return this.appliedBridges().filter((b) => b.treatment.kind === 'bridge');
  }
}

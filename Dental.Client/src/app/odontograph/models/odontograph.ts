/** Canvas odontograph (bardurt/odontograma) persisted by `/api/patients/{id}/odontograph`. */

export interface OdontographDamage {
  tooth: number;
  damage: string;
  surface: string;
  note: string;
}

export interface OdontographDocument {
  id?: string;
  type: 'adult' | 'child';
  damages: OdontographDamage[];
  observations?: string;
  specifications?: string;
}

/** Raw damage row exported by the bardurt Engine.getData(). */
export interface OdontographEngineDamage {
  tooth: number;
  damage: string | number;
  surface?: string;
  note?: string;
  diagnostic?: string;
}

export interface OdontographCanvasTextBox {
  text: string;
}

export interface OdontographCanvasMenuItem {
  id?: number;
  active?: boolean;
  textBox: OdontographCanvasTextBox;
}

export interface OdontographEngine {
  setCanvas(canvas: HTMLCanvasElement): void;
  init(): void;
  start(): void;
  update(): void;
  reset(): void;
  getData(): OdontographEngineDamage[];
  load(tooth: number, damage: number | string, surface: string, note: string): void;
  setDamage(damage: number): void;
  changeView(which: string): void;
  useHtmlToolbar?: boolean;
  selectedDamage?: number;
  onMouseClick(event: MouseEvent): void;
  onMouseMove(event: MouseEvent): void;
  onButtonClick(event: KeyboardEvent): void;
  loadPatientData(
    office: string,
    patient: string,
    number: string,
    treatmentNumber: string,
    treatmentDate: string,
    dentist: string,
    observations: string,
    specs: string,
  ): void;
  adultShowing: boolean;
  observations: string;
  specifications: string;
  menuItems: OdontographCanvasMenuItem[];
  buttons: OdontographCanvasMenuItem[];
  adult: OdontographCanvasMenuItem;
  child: OdontographCanvasMenuItem;
  clear: OdontographCanvasMenuItem;
}

export interface OdontographLayoutConfig {
  teethPerRow: number;
  imgWidth: number;
  imgHeight: number;
  rowGap: number;
  topPad: number;
  lowerLabelSpace: number;
  horizontalPad: number;
}

declare global {
  interface Window {
    Engine: new () => OdontographEngine;
    OdontographUseHtmlToolbar?: boolean;
    OdontographLayout?: OdontographLayoutConfig;
  }
}

export {};

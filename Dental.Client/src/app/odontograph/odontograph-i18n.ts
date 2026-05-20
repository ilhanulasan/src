import { TranslateService } from '@ngx-translate/core';

import { OdontographEngine } from './models/odontograph';

export type OdontographViewKey = 'adult' | 'child' | 'reset';

/** English defaults from bardurt engine — used as fallback when switching languages. */
const DAMAGE_FALLBACK_EN: Record<number, string> = {
  1: 'Caries',
  2: 'Crown',
  3: 'Crown (Tmp)',
  4: 'Missing',
  5: 'Fracture',
  6: 'Implant',
  8: 'Diastema',
  9: 'Extrusion',
  10: 'Microdontia',
  11: 'Filling',
  12: 'Rem Prost',
  13: 'Drifting',
  14: 'Rotation',
  15: 'Fusion',
  16: 'Root Remnant',
  17: 'Macrodontia',
  19: 'Impacted',
  20: 'Pulp',
  21: 'Ectopic',
  22: 'Dyschromic',
  23: 'Rem Orthodo',
  24: 'Eruption',
  25: 'Transpositon',
  27: 'Supernumerary',
  29: 'Prosthesis',
  30: 'Bolt',
  31: 'Edentulism',
  32: 'Fixed Ortho',
  34: 'Fixed Prosth',
  37: 'Worn',
};

const VIEW_FALLBACK_EN: Record<OdontographViewKey, string> = {
  adult: 'Adult',
  child: 'Child',
  reset: 'Reset',
};

export interface OdontographI18nBridge {
  label: (damageId: number, fallback: string) => string;
  viewLabel: (viewKey: OdontographViewKey, fallback: string) => string;
}

declare global {
  interface Window {
    OdontographI18n?: OdontographI18nBridge;
  }
}

function resolve(translate: TranslateService, key: string, fallback: string): string {
  const value = translate.instant(key);
  return value !== key ? value : fallback;
}

export function installOdontographI18n(translate: TranslateService): void {
  window.OdontographI18n = {
    label: (damageId, fallback) =>
      resolve(translate, `odontograph.damage.${damageId}`, fallback),
    viewLabel: (viewKey, fallback) =>
      resolve(translate, `odontograph.view.${viewKey}`, fallback),
  };
}

export function applyOdontographMenuLabels(engine: OdontographEngine, translate: TranslateService): void {
  installOdontographI18n(translate);

  for (const item of engine.menuItems ?? []) {
    const id = item.id ?? 0;
    const fallback = DAMAGE_FALLBACK_EN[id] ?? item.textBox.text;
    item.textBox.text = resolve(translate, `odontograph.damage.${id}`, fallback);
  }

  engine.adult.textBox.text = resolve(translate, 'odontograph.view.adult', VIEW_FALLBACK_EN.adult);
  engine.child.textBox.text = resolve(translate, 'odontograph.view.child', VIEW_FALLBACK_EN.child);
  engine.clear.textBox.text = resolve(translate, 'odontograph.view.reset', VIEW_FALLBACK_EN.reset);

  engine.update();
}

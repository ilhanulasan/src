import { Pathology, Treatment, TreatmentKind } from './models/odontogram';

/** Mirrors OdontoManage default palette / ordering — IDs drive backend JSON and UI badges. */

export function orderedPathologies(): Pathology[] {
  const defs: Pathology[] = [
    { id: 1, color: '#f4d03f' },
    { id: 2, color: '#95a5a6' },
    { id: 3, color: '#3498db' },
    { id: 5, color: '#1abc9c' },
    { id: 4, color: '#2c3e50' },
  ];
  defs.sort((a, b) => {
    const orderMap: Record<number, number> = { 1: 0, 2: 1, 3: 2, 5: 3, 4: 4 };
    return (orderMap[a.id] ?? 99) - (orderMap[b.id] ?? 99);
  });
  return defs.map((p) => ({ ...p }));
}

/** Stable IDs persisted in PostgreSQL payloads; labels from i18n `odontogram.treatment.${id}`. */
export function clinicTreatments(): Treatment[] {
  const rows: { id: number; kind: TreatmentKind }[] = [
    { id: 101, kind: 'extraction' },
    { id: 102, kind: 'endo' },
    { id: 103, kind: 'crown' },
    { id: 104, kind: 'bridge' },
  ];
  return rows.map((r) => ({
    id: r.id,
    kind: r.kind,
  }));
}

const treatmentLookup = new Map(clinicTreatments().map((t) => [t.id, { ...t }]));

export function treatmentById(id: number): Treatment | undefined {
  return treatmentLookup.get(id);
}

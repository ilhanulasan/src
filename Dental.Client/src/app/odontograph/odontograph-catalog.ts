/** Damage tool ids grouped for a uniform toolbar layout (matches bardurt engine). */

export type OdontographToolGroupKey =
  | 'basics'
  | 'restorative'
  | 'position'
  | 'prosthetic'
  | 'anomaly'
  | 'other';

export interface OdontographTool {
  id: number;
}

export interface OdontographToolGroup {
  key: OdontographToolGroupKey;
  tools: OdontographTool[];
}

export const ODONTOGRAPH_TOOL_GROUPS: OdontographToolGroup[] = [
  {
    key: 'basics',
    tools: [{ id: 1 }, { id: 2 }, { id: 3 }, { id: 4 }, { id: 5 }, { id: 8 }],
  },
  {
    key: 'restorative',
    tools: [{ id: 11 }, { id: 12 }, { id: 13 }, { id: 14 }, { id: 15 }, { id: 16 }],
  },
  {
    key: 'position',
    tools: [{ id: 24 }, { id: 25 }, { id: 27 }, { id: 20 }, { id: 29 }, { id: 30 }],
  },
  {
    key: 'prosthetic',
    tools: [{ id: 32 }, { id: 34 }, { id: 6 }, { id: 17 }, { id: 10 }, { id: 22 }],
  },
  {
    key: 'other',
    tools: [{ id: 37 }, { id: 31 }, { id: 21 }, { id: 19 }, { id: 23 }, { id: 9 }],
  },
];

/**
 * Canvas size matched to bardurt layout: upper row at `topPad`, lower row at `topPad + rowGap`.
 * `rowGap` is the Y offset to the lower arch (not an extra gap added after both rows).
 */
export const ODONTOGRAPH_LAYOUT = {
  teethPerRow: 16,
  imgWidth: 40,
  imgHeight: 90,
  rowGap: 210,
  /** Upper-arch labels sit ~42px above the first row. */
  topPad: 48,
  /** Lower-arch labels: 22px below tooth + 20px label height. */
  lowerLabelSpace: 42,
  horizontalPad: 24,
};

export const ODONTOGRAPH_CANVAS_BASE = {
  width:
    ODONTOGRAPH_LAYOUT.teethPerRow * ODONTOGRAPH_LAYOUT.imgWidth +
    ODONTOGRAPH_LAYOUT.horizontalPad * 2,
  height:
    ODONTOGRAPH_LAYOUT.topPad +
    ODONTOGRAPH_LAYOUT.rowGap +
    ODONTOGRAPH_LAYOUT.imgHeight +
    ODONTOGRAPH_LAYOUT.lowerLabelSpace,
};

export const ODONTOGRAPH_CANVAS_SCALE = 1.55;

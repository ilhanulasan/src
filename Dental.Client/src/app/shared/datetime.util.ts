/** Builds an ISO-8601 string with local timezone offset (e.g. 2026-05-19T09:00:00+03:00). */
export function localDateTimePartsToOffsetIso(date: string, time: string): string {
  const [year, month, day] = date.split('-').map(Number);
  const [hours, minutes] = time.split(':').map(Number);
  const dt = new Date(year, month - 1, day, hours, minutes ?? 0, 0, 0);
  return formatOffsetIso(dt);
}

export function formatOffsetIso(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  const tz = -d.getTimezoneOffset();
  const sign = tz >= 0 ? '+' : '-';
  const abs = Math.abs(tz);
  const tzH = pad(Math.floor(abs / 60));
  const tzM = pad(abs % 60);
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00${sign}${tzH}:${tzM}`;
}

export function addMinutesToOffsetIso(iso: string, minutes: number): string {
  const d = new Date(iso);
  d.setMinutes(d.getMinutes() + minutes);
  return formatOffsetIso(d);
}

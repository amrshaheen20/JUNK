export const IS_LOGGING = true;

export function log(...data: unknown[]): void {
  if (!IS_LOGGING) return;

  console.log(...data);
}

export function logError(...data: unknown[]): void {
  if (!IS_LOGGING) return;

  console.error(...data);
}

export function logWarn(...data: unknown[]): void {
  if (!IS_LOGGING) return;

  console.error(...data);
}

export type ClassValue = string | false | null | undefined

/** Joins truthy class names. Later classes win via Tailwind source order, not merging. */
export function cn(...classes: ClassValue[]): string {
  return classes.filter(Boolean).join(' ')
}

import { useCallback, useSyncExternalStore } from 'react'

export type ThemeChoice = 'dark' | 'light' | 'system'
const KEY = 'rng-theme'
const listeners = new Set<() => void>()

function read(): ThemeChoice {
  try {
    const v = localStorage.getItem(KEY)
    if (v === 'dark' || v === 'light') return v
  } catch {
    /* storage unavailable */
  }
  return 'system'
}

export function applyTheme(choice: ThemeChoice): void {
  if (choice === 'system') delete document.documentElement.dataset.theme
  else document.documentElement.dataset.theme = choice
}

export function setTheme(choice: ThemeChoice): void {
  try {
    if (choice === 'system') localStorage.removeItem(KEY)
    else localStorage.setItem(KEY, choice)
  } catch {
    /* storage unavailable */
  }
  applyTheme(choice)
  listeners.forEach((l) => l())
}

const subscribe = (cb: () => void) => {
  listeners.add(cb)
  return () => listeners.delete(cb)
}

/** Stored theme choice; 'system' follows prefers-color-scheme. */
export function useTheme() {
  const theme = useSyncExternalStore(subscribe, read, () => 'system' as ThemeChoice)
  const set = useCallback((t: ThemeChoice) => setTheme(t), [])
  return { theme, setTheme: set }
}

import { Monitor, Moon, Sun } from 'lucide-react'
import { useTheme, type ThemeChoice } from '@/lib/theme'
import { Button } from './Button'

const order: ThemeChoice[] = ['dark', 'light', 'system']
const icons = { dark: Moon, light: Sun, system: Monitor }

/** Cycles dark -> light -> system. */
export function ThemeToggle() {
  const { theme, setTheme } = useTheme()
  const Icon = icons[theme]
  const next = order[(order.indexOf(theme) + 1) % order.length]!
  return (
    <Button
      variant="ghost"
      size="sm"
      aria-label={`Theme: ${theme}. Switch to ${next}`}
      title={`Theme: ${theme}`}
      onClick={() => setTheme(next)}
      className="px-2"
    >
      <Icon aria-hidden className="size-4" />
    </Button>
  )
}

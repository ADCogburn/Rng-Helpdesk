import { cn } from '@/lib/cn'

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger'
export type ButtonSize = 'sm' | 'md' | 'lg'

export const buttonVariants: Record<ButtonVariant, string> = {
  primary:
    'bg-primary text-primary-fg hover:bg-primary-hover shadow-sm shadow-primary/20 border border-transparent',
  secondary: 'bg-raised text-fg hover:bg-line border border-line',
  ghost: 'bg-transparent text-fg hover:bg-raised border border-transparent',
  danger: 'bg-danger text-white hover:brightness-110 border border-transparent',
}

export const buttonSizes: Record<ButtonSize, string> = {
  sm: 'h-8 px-3 text-sm gap-1.5',
  md: 'h-10 px-4 text-sm gap-2',
  lg: 'h-12 px-6 text-base gap-2',
}

/** Class string for styling a router <Link>/<a> like a button. */
export function buttonClass(
  variant: ButtonVariant = 'primary',
  size: ButtonSize = 'md',
  fullWidth = false,
  className?: string,
) {
  return cn(
    'inline-flex items-center justify-center rounded-md font-medium whitespace-nowrap transition',
    'hover:-translate-y-px active:translate-y-0 disabled:pointer-events-none disabled:opacity-50',
    buttonVariants[variant],
    buttonSizes[size],
    fullWidth && 'w-full',
    className,
  )
}

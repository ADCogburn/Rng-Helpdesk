const numberFormat = new Intl.NumberFormat('en-US')
const compactFormat = new Intl.NumberFormat('en-US', {
  notation: 'compact',
  maximumFractionDigits: 1,
})
const dateFormat = new Intl.DateTimeFormat('en-US', { dateStyle: 'medium' })
const dateTimeFormat = new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' })

export const formatNumber = (n: number) => numberFormat.format(n)
export const formatCompact = (n: number) => compactFormat.format(n)
export const formatDate = (iso: string) => dateFormat.format(new Date(iso))
export const formatDateTime = (iso: string) => dateTimeFormat.format(new Date(iso))
/** +50 / -20 / 0 with an explicit sign. */
export const formatDelta = (n: number) =>
  n > 0 ? `+${numberFormat.format(n)}` : numberFormat.format(n)

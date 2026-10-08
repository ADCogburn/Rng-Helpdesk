export interface TimelineItem {
  label: string
  at: string
  /** ISO timestamp used to order newest first. */
  sortKey: string
}

export function Timeline({ items }: { items: TimelineItem[] }) {
  const sorted = [...items].sort((a, b) => b.sortKey.localeCompare(a.sortKey))
  return (
    <ol className="border-line relative ml-2 space-y-4 border-l pl-5">
      {sorted.map((item, i) => (
        <li key={i} className="relative">
          <span
            aria-hidden
            className="bg-primary absolute top-1.5 -left-[1.6rem] size-2.5 rounded-full"
          />
          <p className="font-medium">{item.label}</p>
          <p className="text-muted text-xs">{item.at}</p>
        </li>
      ))}
    </ol>
  )
}

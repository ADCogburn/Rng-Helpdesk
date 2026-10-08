import type { AppRole } from '@/api/types'
import { Badge, type BadgeTone } from '@/components/ui'
import { roleLabel } from '@/lib/ranks'

const tones: Record<AppRole, BadgeTone> = {
  Member: 'neutral',
  Administrator: 'info',
  SuperAdministrator: 'warning',
  Owner: 'primary',
}

export function RoleBadge({ role }: { role: AppRole }) {
  return <Badge tone={tones[role]}>{roleLabel(role)}</Badge>
}

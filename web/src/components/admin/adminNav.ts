import { LayoutDashboard, TrendingUp, UserPlus, Users } from 'lucide-react'

export const adminNav = [
  { to: '/admin', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/admin/members', label: 'Members', icon: Users, end: false },
  { to: '/admin/members/new', label: 'Add member', icon: UserPlus, end: true },
  { to: '/admin/ranks', label: 'Rank thresholds', icon: TrendingUp, end: false },
] as const

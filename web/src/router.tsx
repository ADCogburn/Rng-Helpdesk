import { createBrowserRouter, type RouteObject } from 'react-router'
import { RouteFallback } from '@/components/layout/RouteFallback'

/** Every route is lazy; page files export `Component`. Exported separately so tests can use a memory router. */
export const routes: RouteObject[] = [
  {
    lazy: () => import('./pages/RootLayout'),
    HydrateFallback: RouteFallback,
    children: [
      { index: true, lazy: () => import('./pages/LandingPage') },
      { path: 'login', lazy: () => import('./pages/LoginPage') },
      { path: 'account/change-password', lazy: () => import('./pages/ChangePasswordPage') },
      { path: 'me', lazy: () => import('./pages/MePage') },
      {
        path: 'admin',
        lazy: () => import('./pages/admin/AdminLayout'),
        children: [
          { index: true, lazy: () => import('./pages/admin/AdminDashboardPage') },
          { path: 'members', lazy: () => import('./pages/admin/MembersPage') },
          { path: 'members/new', lazy: () => import('./pages/admin/NewMemberPage') },
          { path: 'members/:id', lazy: () => import('./pages/admin/MemberDetailPage') },
          { path: 'ranks', lazy: () => import('./pages/admin/RankThresholdsPage') },
        ],
      },
      { path: '*', lazy: () => import('./pages/NotFoundPage') },
    ],
  },
]

export const createRouter = () => createBrowserRouter(routes)

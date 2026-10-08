import { createBrowserRouter, type RouteObject } from 'react-router'
import { RequireAdmin, RequireAuth } from '@/auth/guards'
import { RouteFallback } from '@/components/layout/RouteFallback'

/** Every route is lazy; page files export `Component`. Exported separately so tests can use a memory router. */
export const routes: RouteObject[] = [
  {
    lazy: () => import('./pages/RootLayout'),
    HydrateFallback: RouteFallback,
    children: [
      {
        // AuthProvider needs router hooks, so it is a layout route rather than wrapping the router.
        lazy: () => import('./auth/AuthLayout'),
        children: [
          { index: true, lazy: () => import('./pages/LandingPage') },
          { path: 'login', lazy: () => import('./pages/LoginPage') },
          {
            element: <RequireAuth allowPasswordChange />,
            children: [
              {
                path: 'account/change-password',
                lazy: () => import('./pages/ChangePasswordPage'),
              },
            ],
          },
          {
            element: <RequireAuth />,
            children: [
              { path: 'me', lazy: () => import('./pages/MePage') },
              {
                element: <RequireAdmin />,
                children: [
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
                ],
              },
            ],
          },
          { path: '*', lazy: () => import('./pages/NotFoundPage') },
        ],
      },
    ],
  },
]
export const createRouter = () => createBrowserRouter(routes)

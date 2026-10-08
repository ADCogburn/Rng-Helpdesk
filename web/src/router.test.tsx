import { render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it } from 'vitest'
import { routes } from './router'

const cases: [string, string][] = [
  ['/', 'Landing'],
  ['/login', 'Sign in'],
  ['/account/change-password', 'Change password'],
  ['/me', 'My profile'],
  ['/admin', 'Dashboard'],
  ['/admin/members', 'Members'],
  ['/admin/members/new', 'Add member'],
  ['/admin/members/123456789012345678', 'Member detail'],
  ['/admin/ranks', 'Rank thresholds'],
  ['/nope', 'Page not found'],
]

describe('router skeleton', () => {
  it.each(cases)('%s renders', async (path, heading) => {
    const router = createMemoryRouter(routes, { initialEntries: [path] })
    render(<RouterProvider router={router} />)
    expect(await screen.findByRole('heading', { level: 1, name: heading })).toBeInTheDocument()
  })
})

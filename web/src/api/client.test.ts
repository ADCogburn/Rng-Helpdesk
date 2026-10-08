import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { api } from './client'
import { ApiError } from './errors'
import { getToken, setToken, setUnauthorizedHandler } from './session'

function mockFetch(response: Response) {
  const fn = vi.fn().mockResolvedValue(response)
  vi.stubGlobal('fetch', fn)
  return fn
}

describe('api client', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => {
    vi.unstubAllGlobals()
    setUnauthorizedHandler(null)
  })

  it('sends bearer token and JSON body to /api', async () => {
    setToken('abc')
    const fetchFn = mockFetch(new Response(JSON.stringify({ ok: 1 }), { status: 200 }))
    const result = await api.post('/x', { a: 1 })
    expect(result).toEqual({ ok: 1 })
    const [url, init] = fetchFn.mock.calls[0]!
    expect(url).toBe('/api/x')
    expect(init.headers.Authorization).toBe('Bearer abc')
    expect(init.body).toBe('{"a":1}')
  })

  it('treats 204 as undefined and appends query', async () => {
    const fetchFn = mockFetch(new Response(null, { status: 204 }))
    await expect(api.get('/y', { query: { top: 5 } })).resolves.toBeUndefined()
    expect(fetchFn.mock.calls[0]![0]).toBe('/api/y?top=5')
  })

  it('normalizes plain-string and validation errors', async () => {
    mockFetch(new Response('"Not enough points"', { status: 400 }))
    await expect(api.post('/z')).rejects.toMatchObject({
      status: 400,
      message: 'Not enough points',
    })

    mockFetch(
      new Response(JSON.stringify({ errors: { Username: ['Invalid RSN'] } }), { status: 400 }),
    )
    const err = await api.post('/z').catch((e: unknown) => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).fieldErrors).toEqual({ Username: ['Invalid RSN'] })
    expect((err as ApiError).message).toBe('Invalid RSN')
  })

  it('401 clears the session and calls the unauthorized handler', async () => {
    setToken('abc')
    const handler = vi.fn()
    setUnauthorizedHandler(handler)
    mockFetch(new Response(null, { status: 401 }))
    await expect(api.get('/users')).rejects.toMatchObject({ status: 401 })
    expect(getToken()).toBeNull()
    expect(handler).toHaveBeenCalled()
  })

  it('skipAuthRedirect leaves 401 handling to the caller', async () => {
    const handler = vi.fn()
    setUnauthorizedHandler(handler)
    mockFetch(new Response(null, { status: 401 }))
    await expect(api.post('/auth/login', {}, { skipAuthRedirect: true })).rejects.toBeInstanceOf(
      ApiError,
    )
    expect(handler).not.toHaveBeenCalled()
  })

  it('maps network failures to status 0', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('fail')))
    await expect(api.get('/x')).rejects.toMatchObject({ status: 0 })
  })
})

export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors?: Record<string, string[]>

  constructor(status: number, message: string, fieldErrors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

export function errorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (error instanceof ApiError) return error.message
  if (error instanceof Error && error.message) return error.message
  return fallback
}

/** First message for a field (case-insensitive key match), for form display. */
export function fieldError(error: unknown, field: string): string | undefined {
  if (!(error instanceof ApiError) || !error.fieldErrors) return undefined
  const key = Object.keys(error.fieldErrors).find((k) => k.toLowerCase() === field.toLowerCase())
  return key ? error.fieldErrors[key]?.[0] : undefined
}

const DEFAULT_MESSAGES: Record<number, string> = {
  400: 'The request was invalid.',
  401: 'Your session has expired. Please sign in again.',
  403: 'You do not have permission to do that.',
  404: 'Not found.',
  500: 'The server hit an unexpected error.',
}

/** Normalizes the API's error shapes (plain string, ValidationProblemDetails, empty) into ApiError. */
export async function toApiError(response: Response): Promise<ApiError> {
  const fallback = DEFAULT_MESSAGES[response.status] ?? `Request failed (${response.status}).`
  let text = ''
  try {
    text = await response.text()
  } catch {
    /* ignore */
  }
  if (!text) return new ApiError(response.status, fallback)

  let body: unknown = text
  try {
    body = JSON.parse(text)
  } catch {
    /* plain text body */
  }

  if (typeof body === 'string') return new ApiError(response.status, body.trim() || fallback)

  if (body && typeof body === 'object') {
    const obj = body as { errors?: unknown; title?: unknown; detail?: unknown }
    if (obj.errors && typeof obj.errors === 'object') {
      const fieldErrors: Record<string, string[]> = {}
      for (const [key, value] of Object.entries(obj.errors as Record<string, unknown>)) {
        fieldErrors[key] = Array.isArray(value) ? value.map(String) : [String(value)]
      }
      const first = Object.values(fieldErrors)[0]?.[0]
      return new ApiError(response.status, first ?? fallback, fieldErrors)
    }
    if (typeof obj.detail === 'string' && obj.detail)
      return new ApiError(response.status, obj.detail)
    if (typeof obj.title === 'string' && obj.title) return new ApiError(response.status, obj.title)
  }
  return new ApiError(response.status, fallback)
}

import { getSubject } from "@/lib/subject"

const BASE_URL = "/api"

export class ApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.name = "ApiError"
    this.status = status
  }
}

interface RequestOptions {
  method?: string
  body?: unknown
  // When true, attach a fresh X-Idempotency-Key (for create actions).
  idempotent?: boolean
  signal?: AbortSignal
}

/**
 * Extract a human-friendly error message from a failed response.
 * The backend may return ProblemDetails / validation payloads or plain text.
 */
async function extractError(res: Response): Promise<string> {
  const text = await res.text()
  if (!text) return `Request failed with status ${res.status}`
  try {
    const data = JSON.parse(text)
    if (typeof data === "string") return data
    if (data.detail) return data.detail
    if (data.title) return data.title
    if (data.message) return data.message
    if (data.errors) {
      // ASP.NET validation dictionary: { errors: { field: [msgs] } }
      const msgs = Object.values(data.errors as Record<string, string[]>)
        .flat()
        .filter(Boolean)
      if (msgs.length) return msgs.join(" ")
    }
    return text
  } catch {
    return text
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const { method = "GET", body, idempotent, signal } = options

  const headers: Record<string, string> = {
    // Dev auth / multi-tenancy: every request identifies the caller's team.
    "X-Dev-Subject": getSubject(),
  }

  if (body !== undefined) {
    headers["Content-Type"] = "application/json"
  }
  if (idempotent) {
    headers["X-Idempotency-Key"] = crypto.randomUUID()
  }

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  })

  if (!res.ok) {
    throw new ApiError(await extractError(res), res.status)
  }

  // 202 Accepted / 204 No Content -> empty body.
  if (res.status === 202 || res.status === 204) {
    return undefined as T
  }

  const text = await res.text()
  return (text ? JSON.parse(text) : undefined) as T
}

/**
 * Like {@link apiFetch} but resolves with the raw {@link Response} so callers
 * can read headers (e.g. the `Location` of a freshly-created resource that the
 * 202 body does not include).
 */
export async function apiFetchRaw(
  path: string,
  options: RequestOptions = {},
): Promise<Response> {
  const { method = "GET", body, idempotent, signal } = options

  const headers: Record<string, string> = {
    "X-Dev-Subject": getSubject(),
  }
  if (body !== undefined) headers["Content-Type"] = "application/json"
  if (idempotent) headers["X-Idempotency-Key"] = crypto.randomUUID()

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  })

  if (!res.ok) {
    throw new ApiError(await extractError(res), res.status)
  }
  return res
}

/** Extract a trailing GUID from a URL/path, if present. */
export function idFromLocation(location: string | null): string | null {
  if (!location) return null
  const match = location.match(
    /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
  )
  return match ? match[0] : null
}

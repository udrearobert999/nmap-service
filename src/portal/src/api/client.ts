import { buildAuthHeaders } from "./auth-headers"

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
  idempotent?: boolean
  signal?: AbortSignal
}

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
  const headers: Record<string, string> = { ...(await buildAuthHeaders()) }
  if (body !== undefined) headers["Content-Type"] = "application/json"
  if (idempotent) headers["X-Idempotency-Key"] = crypto.randomUUID()

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  })

  if (!res.ok) throw new ApiError(await extractError(res), res.status)
  if (res.status === 202 || res.status === 204) return undefined as T
  const text = await res.text()
  return (text ? JSON.parse(text) : undefined) as T
}

export async function apiFetchRaw(
  path: string,
  options: RequestOptions = {},
): Promise<Response> {
  const { method = "GET", body, idempotent, signal } = options
  const headers: Record<string, string> = { ...(await buildAuthHeaders()) }
  if (body !== undefined) headers["Content-Type"] = "application/json"
  if (idempotent) headers["X-Idempotency-Key"] = crypto.randomUUID()

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  })

  if (!res.ok) throw new ApiError(await extractError(res), res.status)
  return res
}

export function idFromLocation(location: string | null): string | null {
  if (!location) return null
  const match = location.match(
    /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
  )
  return match ? match[0] : null
}

export type TokenGetter = () => Promise<string | null>

export interface AuthState {
  userId: string | null
  orgId: string | null
  orgRole: string | null
  email: string | null
  getToken: TokenGetter
}

let current: AuthState = {
  userId: null,
  orgId: null,
  orgRole: null,
  email: null,
  getToken: async () => null,
}

export function setAuth(next: AuthState): void {
  current = next
}

export async function buildAuthHeaders(): Promise<Record<string, string>> {
  const headers: Record<string, string> = {}
  if (current.userId) headers["X-Dev-Subject"] = current.userId
  if (current.orgId) headers["X-Dev-Org"] = current.orgId
  if (current.orgRole) headers["X-Dev-Role"] = current.orgRole
  if (current.email) headers["X-Dev-Email"] = current.email
  try {
    const token = await current.getToken()
    if (token) headers["Authorization"] = `Bearer ${token}`
  } catch {
    // token fetch can fail transiently; the dev backend does not require it
  }
  return headers
}

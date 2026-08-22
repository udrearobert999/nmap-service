// Dev-mode "identity". The backend (Development mode) trusts the
// `X-Dev-Subject` header and maps each subject to its own tenant/team,
// so switching the subject switches the tenant you see.

const STORAGE_KEY = "portal.devSubject"
const DEFAULT_SUBJECT = "teamA|user1"

export function getSubject(): string {
  if (typeof localStorage === "undefined") return DEFAULT_SUBJECT
  return localStorage.getItem(STORAGE_KEY) ?? DEFAULT_SUBJECT
}

export function setSubject(subject: string): void {
  localStorage.setItem(STORAGE_KEY, subject)
}

export { DEFAULT_SUBJECT }

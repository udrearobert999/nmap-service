import { createContext, useCallback, useContext, useState } from "react"
import { useQueryClient } from "@tanstack/react-query"
import { getSubject, setSubject as persistSubject } from "./subject"

interface SubjectContextValue {
  subject: string
  setSubject: (subject: string) => void
}

const SubjectContext = createContext<SubjectContextValue | null>(null)

export function SubjectProvider({ children }: { children: React.ReactNode }) {
  const qc = useQueryClient()
  const [subject, setSubjectState] = useState<string>(() => getSubject())

  const setSubject = useCallback(
    (next: string) => {
      const trimmed = next.trim()
      if (!trimmed) return
      persistSubject(trimmed)
      setSubjectState(trimmed)
      // Switching tenant: drop cached data so every view refetches as the
      // new subject (query keys are also scoped by subject).
      qc.clear()
    },
    [qc],
  )

  return (
    <SubjectContext.Provider value={{ subject, setSubject }}>
      {children}
    </SubjectContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useSubject(): SubjectContextValue {
  const ctx = useContext(SubjectContext)
  if (!ctx) throw new Error("useSubject must be used within a SubjectProvider")
  return ctx
}

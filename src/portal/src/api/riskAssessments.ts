import { useMutation, useQuery } from "@tanstack/react-query"
import { apiFetch, apiFetchRaw, idFromLocation } from "./client"
import { getSubject } from "@/lib/subject"
import type { RiskAssessment } from "./types"

/**
 * Requests a risk assessment for a scan. The backend returns 202 Accepted with
 * an empty body; the id of the created assessment is read from the `Location`
 * response header when present (returns `null` otherwise, in which case the UI
 * asks the user to supply the id to track).
 */
export async function createRiskAssessment(
  scanId: string,
): Promise<{ id: string | null }> {
  const res = await apiFetchRaw(`/risk-assessments`, {
    method: "POST",
    body: { scanId },
    idempotent: true,
  })
  return { id: idFromLocation(res.headers.get("Location")) }
}

export function getRiskAssessment(id: string): Promise<RiskAssessment> {
  return apiFetch<RiskAssessment>(`/risk-assessments/${id}`)
}

export function useCreateRiskAssessment() {
  return useMutation({
    mutationFn: (scanId: string) => createRiskAssessment(scanId),
  })
}

/** Poll a risk assessment until it reaches a terminal state. */
export function useRiskAssessment(id: string | undefined) {
  return useQuery({
    queryKey: ["risk-assessment", getSubject(), id],
    queryFn: () => getRiskAssessment(id as string),
    enabled: !!id,
    refetchInterval: (query) => {
      const data = query.state.data
      if (!data) return 3000
      return data.status === "Pending" || data.status === "Running"
        ? 3000
        : false
    },
  })
}

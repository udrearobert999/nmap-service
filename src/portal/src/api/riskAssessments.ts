import { useMutation, useQuery } from "@tanstack/react-query"
import { ApiError, apiFetch, apiFetchRaw, idFromLocation } from "./client"
import type { Paged, RiskAssessment, RiskStatus } from "./types"

export interface ListRiskAssessmentsParams {
  pageNumber: number
  pageSize: number
  status?: RiskStatus
  orderBy?: string
  orderDirection?: "asc" | "desc"
}

export function listRiskAssessments(
  params: ListRiskAssessmentsParams,
): Promise<Paged<RiskAssessment>> {
  const q = new URLSearchParams()
  q.set("pageNumber", String(params.pageNumber))
  q.set("pageSize", String(params.pageSize))
  if (params.status) q.set("status", params.status)
  if (params.orderBy) q.set("orderBy", params.orderBy)
  if (params.orderDirection) q.set("orderDirection", params.orderDirection)
  return apiFetch<Paged<RiskAssessment>>(`/risk-assessments?${q.toString()}`)
}

export async function createRiskAssessment(
  scanId: string,
): Promise<{ id: string | null }> {
  const res = await apiFetchRaw(`/scans/${scanId}/risk-assessments`, {
    method: "POST",
    idempotent: true,
  })
  return { id: idFromLocation(res.headers.get("Location")) }
}

export function getRiskAssessment(id: string): Promise<RiskAssessment> {
  return apiFetch<RiskAssessment>(`/risk-assessments/${id}`)
}

export function getLatestRiskAssessment(
  scanId: string,
): Promise<RiskAssessment | null> {
  return apiFetch<RiskAssessment>(`/scans/${scanId}/risk-assessment`).catch(
    (err) => {
      if (err instanceof ApiError && err.status === 404) return null
      throw err
    },
  )
}

export function useCreateRiskAssessment() {
  return useMutation({
    mutationFn: (scanId: string) => createRiskAssessment(scanId),
  })
}

export function useLatestRiskAssessment(scanId: string | undefined) {
  return useQuery({
    queryKey: ["risk-assessment-latest", scanId],
    queryFn: () => getLatestRiskAssessment(scanId as string),
    enabled: !!scanId,
    refetchInterval: (query) => {
      const data = query.state.data
      if (!data) return false
      return data.status === "Pending" || data.status === "Running" ? 3000 : false
    },
  })
}

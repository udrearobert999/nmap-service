import { useQuery } from "@tanstack/react-query"
import { apiFetch } from "./client"
import type { RiskTrend } from "./types"

export function getRiskTrend(target: string): Promise<RiskTrend> {
  return apiFetch<RiskTrend>(
    `/risk-assessments/trend/${encodeURIComponent(target)}`,
  )
}

export function useRiskTrend(target: string) {
  return useQuery({
    queryKey: ["risk-assessments", "trend", target],
    queryFn: () => getRiskTrend(target),
    enabled: target.trim().length > 0,
    retry: false,
  })
}

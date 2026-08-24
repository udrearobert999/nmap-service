import { useQuery } from "@tanstack/react-query"
import { apiFetch } from "./client"

export interface DashboardStats {
  totalScans: number
  totalAssessments: number
  completedAssessments: number
  avgRiskScore: number | null
}

const EMPTY_STATS: DashboardStats = {
  totalScans: 0,
  totalAssessments: 0,
  completedAssessments: 0,
  avgRiskScore: null,
}

export function getDashboardSummary(): Promise<DashboardStats> {
  return apiFetch<DashboardStats>("/dashboard/summary")
}

export function useDashboard() {
  const { data, isLoading } = useQuery({
    queryKey: ["dashboard", "summary"],
    queryFn: getDashboardSummary,
    refetchInterval: 10_000,
  })

  return { stats: data ?? EMPTY_STATS, isLoading }
}

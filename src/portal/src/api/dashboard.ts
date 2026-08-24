import { useQueries } from "@tanstack/react-query"
import { listScans } from "./scans"
import { listRiskAssessments } from "./riskAssessments"

export interface DashboardStats {
  totalScans: number
  totalAssessments: number
  completedAssessments: number
  avgRiskScore: number | null
}

const PAGE = { pageNumber: 1, pageSize: 25 } as const

export function useDashboard() {
  const [scansQuery, assessmentsQuery, completedQuery] = useQueries({
    queries: [
      {
        queryKey: ["dashboard", "scans"],
        queryFn: () =>
          listScans({ ...PAGE, orderBy: "createdAt", orderDirection: "desc" }),
        refetchInterval: 10_000,
      },
      {
        queryKey: ["dashboard", "assessments"],
        queryFn: () =>
          listRiskAssessments({
            ...PAGE,
            orderBy: "requestedAt",
            orderDirection: "desc",
          }),
        refetchInterval: 10_000,
      },
      {
        queryKey: ["dashboard", "assessments", "completed"],
        queryFn: () =>
          listRiskAssessments({
            ...PAGE,
            status: "Completed",
            orderBy: "requestedAt",
            orderDirection: "desc",
          }),
        refetchInterval: 10_000,
      },
    ],
  })

  const completedItems = completedQuery.data?.items ?? []
  const scoredItems = completedItems.filter((a) => a.overallRiskScore != null)
  const avgRiskScore =
    scoredItems.length > 0
      ? Math.round(
          (scoredItems.reduce((sum, a) => sum + (a.overallRiskScore ?? 0), 0) /
            scoredItems.length) *
            10,
        ) / 10
      : null

  const stats: DashboardStats = {
    totalScans: scansQuery.data?.total ?? 0,
    totalAssessments: assessmentsQuery.data?.total ?? 0,
    completedAssessments: completedQuery.data?.total ?? 0,
    avgRiskScore,
  }

  return {
    stats,
    isLoading:
      scansQuery.isLoading ||
      assessmentsQuery.isLoading ||
      completedQuery.isLoading,
  }
}

import { useQuery } from "@tanstack/react-query"
import { listScans } from "./scans"
import { listRiskAssessments } from "./riskAssessments"
import type { Scan } from "./types"

export interface DashboardStats {
  totalScans: number
  totalAssessments: number
  completedAssessments: number
  avgRiskScore: number | null
}

export interface ChartPoint {
  date: string
  scans: number
}

const PAGE = { pageNumber: 1, pageSize: 25 } as const

export function useDashboard() {
  const scansQuery = useQuery({
    queryKey: ["dashboard", "scans"],
    queryFn: () =>
      listScans({ ...PAGE, orderBy: "createdAt", orderDirection: "desc" }),
    refetchInterval: 10_000,
  })

  const assessmentsQuery = useQuery({
    queryKey: ["dashboard", "assessments"],
    queryFn: () =>
      listRiskAssessments({
        ...PAGE,
        orderBy: "requestedAt",
        orderDirection: "desc",
      }),
    refetchInterval: 10_000,
  })

  const completedQuery = useQuery({
    queryKey: ["dashboard", "assessments", "completed"],
    queryFn: () =>
      listRiskAssessments({
        ...PAGE,
        status: "Completed",
        orderBy: "requestedAt",
        orderDirection: "desc",
      }),
    refetchInterval: 10_000,
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

  const chartData = buildChartData(scansQuery.data?.items ?? [], 14)

  return {
    stats,
    chartData,
    isLoading:
      scansQuery.isLoading ||
      assessmentsQuery.isLoading ||
      completedQuery.isLoading,
  }
}

function buildChartData(scans: Scan[], days: number): ChartPoint[] {
  const counts = new Map<string, number>()
  for (const scan of scans) {
    const key = scan.createdAt.slice(0, 10)
    counts.set(key, (counts.get(key) ?? 0) + 1)
  }

  const points: ChartPoint[] = []
  const today = new Date()
  for (let i = days - 1; i >= 0; i--) {
    const d = new Date(today)
    d.setDate(today.getDate() - i)
    const key = d.toISOString().slice(0, 10)
    points.push({ date: key, scans: counts.get(key) ?? 0 })
  }
  return points
}

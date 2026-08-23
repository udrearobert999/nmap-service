import { useDashboard } from "@/api/dashboard"
import { SectionCards } from "@/components/SectionCards"
import { ScansChart } from "@/components/ScansChart"
import { Skeleton } from "@/components/ui/skeleton"

export function DashboardPage() {
  const { stats, chartData, isLoading } = useDashboard()

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
        <p className="text-sm text-muted-foreground">
          Overview of your team's scanning and risk posture.
        </p>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      ) : (
        <SectionCards stats={stats} />
      )}

      <ScansChart data={chartData} />
    </div>
  )
}

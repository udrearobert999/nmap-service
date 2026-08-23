import type { ReactNode } from "react"
import { Activity, ListChecks, Radar, ShieldAlert } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { cn } from "@/lib/utils"
import type { DashboardStats } from "@/api/dashboard"

interface StatCardProps {
  label: string
  value: ReactNode
  badge?: { text: string; className?: string }
  icon: ReactNode
  footer: string
}

function StatCard({ label, value, badge, icon, footer }: StatCardProps) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between gap-2">
          <CardDescription className="flex items-center gap-2">
            {icon}
            {label}
          </CardDescription>
          {badge && (
            <Badge
              variant="outline"
              className={cn("shrink-0 font-mono text-xs", badge.className)}
            >
              {badge.text}
            </Badge>
          )}
        </div>
        <CardTitle className="text-3xl font-semibold tabular-nums tracking-tight">
          {value}
        </CardTitle>
      </CardHeader>
      <CardFooter className="text-xs text-muted-foreground">{footer}</CardFooter>
    </Card>
  )
}

function riskBadgeClass(score: number): string {
  if (score >= 7) return "border-transparent bg-red-600 text-white"
  if (score >= 4) return "border-transparent bg-amber-500 text-white"
  return "border-transparent bg-green-600 text-white"
}

export function SectionCards({ stats }: { stats: DashboardStats }) {
  const { totalScans, totalAssessments, completedAssessments, avgRiskScore } =
    stats

  const completionRate =
    totalAssessments > 0
      ? `${Math.round((completedAssessments / totalAssessments) * 100)}%`
      : "—"

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <StatCard
        label="Total Scans"
        value={totalScans.toLocaleString()}
        icon={<Radar className="h-4 w-4" />}
        footer="All scans for this team"
      />
      <StatCard
        label="Risk Assessments"
        value={totalAssessments.toLocaleString()}
        badge={{ text: `${completionRate} done` }}
        icon={<ShieldAlert className="h-4 w-4" />}
        footer={`${completedAssessments.toLocaleString()} completed`}
      />
      <StatCard
        label="Completed Assessments"
        value={completedAssessments.toLocaleString()}
        icon={<ListChecks className="h-4 w-4" />}
        footer="Assessments with results"
      />
      <StatCard
        label="Avg Risk Score"
        value={avgRiskScore ?? "—"}
        badge={
          avgRiskScore != null
            ? { text: avgRiskScore.toFixed(1), className: riskBadgeClass(avgRiskScore) }
            : undefined
        }
        icon={<Activity className="h-4 w-4" />}
        footer="Across recent completed"
      />
    </div>
  )
}

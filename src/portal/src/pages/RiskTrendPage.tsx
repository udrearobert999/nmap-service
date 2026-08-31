import { useMemo, useState } from "react"
import { Line, LineChart, CartesianGrid, XAxis, YAxis, ReferenceLine } from "recharts"
import { useScans } from "@/api/scans"
import { useRiskTrend } from "@/api/riskTrend"
import { formatDate } from "@/lib/format"
import { cn } from "@/lib/utils"
import { ApiError } from "@/api/client"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart"

const chartConfig = {
  overallRiskScore: { label: "Risk score", color: "#dc2626" },
} satisfies ChartConfig

function riskScoreClass(score: number): string {
  if (score >= 7) return "bg-red-600 text-white hover:bg-red-600 border-transparent"
  if (score >= 4)
    return "bg-amber-500 text-white hover:bg-amber-500 border-transparent"
  return "bg-green-600 text-white hover:bg-green-600 border-transparent"
}

export function RiskTrendPage() {
  const [target, setTarget] = useState("")

  const { data: recent } = useScans({
    pageNumber: 1,
    pageSize: 25,
    orderBy: "createdAt",
    orderDirection: "desc",
  })

  const targets = useMemo(() => {
    const seen = new Set<string>()
    for (const s of recent?.items ?? []) seen.add(s.target)
    return [...seen]
  }, [recent])

  const { data: trend, isLoading, isError, error } = useRiskTrend(target)
  const points = trend?.points ?? []
  const latest = points.length > 0 ? points[points.length - 1] : undefined

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Risk trend</h1>
        <p className="text-sm text-muted-foreground">
          Overall risk score for a target across its completed risk
          assessments, over time.
        </p>
      </div>

      <div className="flex flex-wrap items-end gap-4">
        <div className="space-y-1.5">
          <Label>Target</Label>
          <Select value={target} onValueChange={setTarget}>
            <SelectTrigger className="w-[260px]">
              <SelectValue placeholder="Select a target…" />
            </SelectTrigger>
            <SelectContent>
              {targets.map((t) => (
                <SelectItem key={t} value={t} className="font-mono">
                  {t}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {latest && (
          <div className="flex items-baseline gap-2">
            <span className="text-sm text-muted-foreground">Current</span>
            <Badge
              variant="outline"
              className={cn(riskScoreClass(latest.overallRiskScore ?? 0))}
            >
              {latest.overallRiskScore?.toFixed(1) ?? "—"}
            </Badge>
          </div>
        )}
      </div>

      {!target && (
        <p className="text-sm text-muted-foreground">
          Pick a target above. A target needs at least one completed risk
          assessment to show a trend.
        </p>
      )}

      {target && isLoading && <Skeleton className="h-72 w-full" />}

      {target && isError && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-destructive">
              {error instanceof ApiError
                ? error.message
                : "Could not load the risk trend for this target."}
            </p>
          </CardContent>
        </Card>
      )}

      {target && trend && points.length === 0 && !isLoading && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">
              No completed risk assessments yet for this target.
            </p>
          </CardContent>
        </Card>
      )}

      {target && points.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              {target}{" "}
              <span className="font-mono text-sm text-muted-foreground">
                ({points.length} assessment{points.length === 1 ? "" : "s"})
              </span>
            </CardTitle>
          </CardHeader>
          <div className="px-2 pb-4 sm:px-6">
            <ChartContainer
              config={chartConfig}
              className="aspect-auto h-[320px] w-full"
            >
              <LineChart data={points} margin={{ left: 4, right: 4, top: 8 }}>
                <CartesianGrid vertical={false} strokeDasharray="3 3" />
                <XAxis
                  dataKey="completedAt"
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  minTickGap={24}
                  tickFormatter={(v) =>
                    new Date(String(v)).toLocaleDateString(undefined, {
                      month: "short",
                      day: "numeric",
                    })
                  }
                />
                <YAxis
                  domain={[0, 10]}
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  width={28}
                />
                <ReferenceLine y={7} stroke="#dc2626" strokeDasharray="3 3" strokeOpacity={0.4} />
                <ReferenceLine y={4} stroke="#f59e0b" strokeDasharray="3 3" strokeOpacity={0.4} />
                <ChartTooltip
                  cursor={false}
                  content={
                    <ChartTooltipContent
                      labelFormatter={(value) => formatDate(String(value))}
                      indicator="dot"
                    />
                  }
                />
                <Line
                  dataKey="overallRiskScore"
                  type="monotone"
                  stroke="var(--color-overallRiskScore)"
                  strokeWidth={2}
                  isAnimationActive={false}
                  dot={{ r: 3 }}
                  connectNulls
                />
              </LineChart>
            </ChartContainer>
          </div>
        </Card>
      )}
    </div>
  )
}

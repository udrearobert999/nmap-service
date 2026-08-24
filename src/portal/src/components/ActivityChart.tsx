import { useEffect, useMemo, useState } from "react"
import { Area, AreaChart, CartesianGrid, XAxis } from "recharts"
import { useActivity, type Bucket } from "@/api/activity"
import { cn } from "@/lib/utils"
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart"

type SeriesKey = "scans" | "assessments"

const chartConfig = {
  scans: { label: "Scans", color: "#e5e5e5" },
  assessments: { label: "Assessments", color: "#f59e0b" },
} satisfies ChartConfig

const RANGES = [
  { key: "24h", label: "24h", ms: 24 * 60 * 60 * 1000, bucket: "hour" as Bucket },
  { key: "7d", label: "7d", ms: 7 * 24 * 60 * 60 * 1000, bucket: "day" as Bucket },
  { key: "14d", label: "14d", ms: 14 * 24 * 60 * 60 * 1000, bucket: "day" as Bucket },
  { key: "30d", label: "30d", ms: 30 * 24 * 60 * 60 * 1000, bucket: "day" as Bucket },
] as const

type RangeKey = (typeof RANGES)[number]["key"]

function formatTick(value: string, bucket: Bucket): string {
  const d = new Date(value)
  return bucket === "hour"
    ? d.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" })
    : d.toLocaleDateString(undefined, { month: "short", day: "numeric" })
}

export function ActivityChart() {
  const [rangeKey, setRangeKey] = useState<RangeKey>("14d")
  const [visible, setVisible] = useState<Record<SeriesKey, boolean>>({
    scans: true,
    assessments: true,
  })
  const [minuteTick, setMinuteTick] = useState(() =>
    Math.floor(Date.now() / 60_000),
  )

  useEffect(() => {
    const id = setInterval(
      () => setMinuteTick(Math.floor(Date.now() / 60_000)),
      60_000,
    )
    return () => clearInterval(id)
  }, [])

  const preset = RANGES.find((r) => r.key === rangeKey) ?? RANGES[2]

  const range = useMemo(() => {
    const toMs = minuteTick * 60_000
    return {
      from: new Date(toMs - preset.ms).toISOString(),
      to: new Date(toMs).toISOString(),
      bucket: preset.bucket,
    }
  }, [minuteTick, preset.ms, preset.bucket])

  const { data } = useActivity(range)
  const points = data ?? []

  const toggle = (key: SeriesKey) =>
    setVisible((v) => ({ ...v, [key]: !v[key] }))

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <CardTitle>Activity</CardTitle>
          <CardDescription>
            Scans and risk assessments started over time
          </CardDescription>
        </div>
        <div className="flex items-center gap-1 rounded-md border p-0.5">
          {RANGES.map((r) => (
            <button
              key={r.key}
              onClick={() => setRangeKey(r.key)}
              className={cn(
                "rounded px-2.5 py-1 text-xs font-medium transition-colors",
                r.key === rangeKey
                  ? "bg-muted text-foreground"
                  : "text-muted-foreground hover:text-foreground",
              )}
            >
              {r.label}
            </button>
          ))}
        </div>
      </CardHeader>

      <div className="flex gap-4 px-6 pb-2">
        {(Object.keys(chartConfig) as SeriesKey[]).map((key) => (
          <button
            key={key}
            onClick={() => toggle(key)}
            className={cn(
              "flex items-center gap-1.5 text-xs transition-opacity",
              visible[key] ? "opacity-100" : "opacity-40",
            )}
          >
            <span
              className="h-2.5 w-2.5 rounded-[2px]"
              style={{ backgroundColor: chartConfig[key].color }}
            />
            {chartConfig[key].label}
          </button>
        ))}
      </div>

      <div className="px-2 pb-4 sm:px-6">
        <ChartContainer config={chartConfig} className="aspect-auto h-[260px] w-full">
          <AreaChart data={points} margin={{ left: 4, right: 4, top: 8 }}>
            <defs>
              <linearGradient id="fillScans" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="var(--color-scans)" stopOpacity={0.6} />
                <stop offset="95%" stopColor="var(--color-scans)" stopOpacity={0.04} />
              </linearGradient>
              <linearGradient id="fillAssessments" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="var(--color-assessments)" stopOpacity={0.5} />
                <stop offset="95%" stopColor="var(--color-assessments)" stopOpacity={0.03} />
              </linearGradient>
            </defs>
            <CartesianGrid vertical={false} strokeDasharray="3 3" />
            <XAxis
              dataKey="bucket"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              minTickGap={24}
              tickFormatter={(v) => formatTick(String(v), preset.bucket)}
            />
            <ChartTooltip
              cursor={false}
              content={
                <ChartTooltipContent
                  labelFormatter={(value) => formatTick(String(value), preset.bucket)}
                  indicator="dot"
                />
              }
            />
            {visible.scans && (
              <Area
                dataKey="scans"
                type="monotone"
                stroke="var(--color-scans)"
                strokeWidth={2}
                fill="url(#fillScans)"
                isAnimationActive={false}
                dot={false}
              />
            )}
            {visible.assessments && (
              <Area
                dataKey="assessments"
                type="monotone"
                stroke="var(--color-assessments)"
                strokeWidth={2}
                fill="url(#fillAssessments)"
                isAnimationActive={false}
                dot={false}
              />
            )}
          </AreaChart>
        </ChartContainer>
      </div>
    </Card>
  )
}

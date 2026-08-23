import { Area, AreaChart, CartesianGrid, XAxis } from "recharts"
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
import type { ChartPoint } from "@/api/dashboard"

const chartConfig = {
  scans: {
    label: "Scans",
    color: "hsl(var(--chart-1))",
  },
} satisfies ChartConfig

function formatDay(value: string): string {
  const d = new Date(value)
  return d.toLocaleDateString(undefined, { month: "short", day: "numeric" })
}

export function ScansChart({ data }: { data: ChartPoint[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Scan activity</CardTitle>
        <CardDescription>Scans started over the last 14 days</CardDescription>
      </CardHeader>
      <div className="px-2 pb-4 sm:px-6">
        <ChartContainer config={chartConfig} className="aspect-auto h-[260px] w-full">
          <AreaChart data={data} margin={{ left: 4, right: 4, top: 8 }}>
            <defs>
              <linearGradient id="fillScans" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="var(--color-scans)" stopOpacity={0.7} />
                <stop offset="95%" stopColor="var(--color-scans)" stopOpacity={0.05} />
              </linearGradient>
            </defs>
            <CartesianGrid vertical={false} strokeDasharray="3 3" />
            <XAxis
              dataKey="date"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              minTickGap={24}
              tickFormatter={formatDay}
            />
            <ChartTooltip
              cursor={false}
              content={
                <ChartTooltipContent
                  labelFormatter={(value) => formatDay(String(value))}
                  indicator="dot"
                />
              }
            />
            <Area
              dataKey="scans"
              type="monotone"
              stroke="var(--color-scans)"
              strokeWidth={2}
              fill="url(#fillScans)"
              isAnimationActive={false}
              dot={false}
            />
          </AreaChart>
        </ChartContainer>
      </div>
    </Card>
  )
}

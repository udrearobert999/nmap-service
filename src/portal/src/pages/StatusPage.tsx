import { useHealth } from "@/api/health"
import { cn } from "@/lib/utils"
import { Card, CardContent } from "@/components/ui/card"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"

type DotState = "up" | "down" | "unknown"

const SERVICES = [
  {
    key: "scanning" as const,
    label: "Scanning",
    description: "nmap scan worker",
  },
  {
    key: "assessment" as const,
    label: "Assessment",
    description: "vulnerability-intelligence service",
  },
]

function stateLabel(state: DotState) {
  if (state === "up") return "online"
  if (state === "down") return "offline"
  return "unknown"
}

function ServiceRow({
  label,
  description,
  state,
}: {
  label: string
  description: string
  state: DotState
}) {
  return (
    <div className="flex items-center justify-between border-b py-4 last:border-b-0">
      <div className="flex items-center gap-3">
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <span
                className={cn(
                  "h-2.5 w-2.5 shrink-0 rounded-full",
                  state === "up" && "bg-green-500",
                  state === "down" && "bg-red-500",
                  state === "unknown" && "bg-muted-foreground/40",
                )}
              />
            </TooltipTrigger>
            <TooltipContent>
              {label} — {stateLabel(state)}
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
        <div>
          <div className="text-sm font-medium">{label}</div>
          <div className="text-xs text-muted-foreground">{description}</div>
        </div>
      </div>
      <span
        className={cn(
          "font-mono text-xs uppercase",
          state === "up" && "text-green-500",
          state === "down" && "text-red-500",
          state === "unknown" && "text-muted-foreground",
        )}
      >
        {stateLabel(state)}
      </span>
    </div>
  )
}

export function StatusPage() {
  const { data, isError, dataUpdatedAt } = useHealth()

  const resolve = (up: boolean | undefined): DotState => {
    if (isError) return "down"
    if (up === undefined) return "unknown"
    return up ? "up" : "down"
  }

  const secondsAgo = dataUpdatedAt
    ? Math.max(0, Math.round((Date.now() - dataUpdatedAt) / 1000))
    : null

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">System status</h1>
        <p className="text-sm text-muted-foreground">
          Live connectivity for the background services, refreshed every 10s.
        </p>
      </div>

      <Card>
        <CardContent className="py-2">
          {SERVICES.map((s) => (
            <ServiceRow
              key={s.key}
              label={s.label}
              description={s.description}
              state={resolve(data ? data.services[s.key] === "up" : undefined)}
            />
          ))}
        </CardContent>
      </Card>

      <p className="text-xs text-muted-foreground">
        {secondsAgo === null
          ? "Checking…"
          : `Last checked ${secondsAgo}s ago`}
      </p>
    </div>
  )
}

import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import type { RiskStatus, ScanStatus } from "@/api/types"

// Pending = muted, Running = blue/animated, Completed = green, Failed = red.
const STYLES: Record<ScanStatus | RiskStatus, string> = {
  Pending: "bg-muted text-muted-foreground hover:bg-muted",
  Running:
    "bg-blue-500 text-white hover:bg-blue-500 animate-pulse border-transparent",
  Completed:
    "bg-green-600 text-white hover:bg-green-600 border-transparent",
  Failed:
    "bg-red-600 text-white hover:bg-red-600 border-transparent",
}

export function StatusBadge({
  status,
  className,
}: {
  status: ScanStatus | RiskStatus
  className?: string
}) {
  return (
    <Badge className={cn(STYLES[status], className)} variant="outline">
      {status}
    </Badge>
  )
}

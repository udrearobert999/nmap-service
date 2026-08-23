import { useHealth } from "@/api/health"
import { cn } from "@/lib/utils"

type DotState = "up" | "down" | "unknown"

function Dot({ label, state }: { label: string; state: DotState }) {
  return (
    <span className="flex items-center gap-1.5" title={`${label}: ${state}`}>
      <span
        className={cn(
          "h-2.5 w-2.5 rounded-full",
          state === "up" && "bg-green-500",
          state === "down" && "bg-red-500",
          state === "unknown" && "bg-muted-foreground/40",
        )}
      />
      <span className="hidden text-xs text-muted-foreground md:inline">
        {label}
      </span>
    </span>
  )
}

export function ConnectivityIndicator() {
  const { data, isError } = useHealth()

  const resolve = (up: boolean | undefined): DotState => {
    if (isError) return "down"
    if (up === undefined) return "unknown"
    return up ? "up" : "down"
  }

  return (
    <div className="flex items-center gap-3" aria-label="Backend connectivity">
      <Dot label="DB" state={resolve(data ? data.database === "up" : undefined)} />
      <Dot
        label="Scanning"
        state={resolve(data ? data.services.scanning === "up" : undefined)}
      />
      <Dot
        label="Assessment"
        state={resolve(data ? data.services.assessment === "up" : undefined)}
      />
    </div>
  )
}

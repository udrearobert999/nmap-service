import { useMemo, useState } from "react"
import { useScans } from "@/api/scans"
import { useScanDiff } from "@/api/diff"
import { formatDate } from "@/lib/format"
import { cn } from "@/lib/utils"
import { ApiError } from "@/api/client"
import type { PortStateChange, ScanResult } from "@/api/types"
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

const AUTO = "__auto__"

function ResultTable({ rows }: { rows: ScanResult[] }) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Port</TableHead>
          <TableHead>Protocol</TableHead>
          <TableHead>Service</TableHead>
          <TableHead>State</TableHead>
          <TableHead>Product / Version</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((r) => (
          <TableRow key={`${r.port}/${r.protocol}`}>
            <TableCell className="font-mono">{r.port}</TableCell>
            <TableCell>{r.protocol}</TableCell>
            <TableCell>{r.service}</TableCell>
            <TableCell>{r.state}</TableCell>
            <TableCell className="text-muted-foreground">
              {[r.product, r.version].filter(Boolean).join(" ") || "—"}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

function ChangeTable({ rows }: { rows: PortStateChange[] }) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Port</TableHead>
          <TableHead>Protocol</TableHead>
          <TableHead>Was</TableHead>
          <TableHead>Now</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((r) => (
          <TableRow key={`${r.port}/${r.protocol}`}>
            <TableCell className="font-mono">{r.port}</TableCell>
            <TableCell>{r.protocol}</TableCell>
            <TableCell className="text-muted-foreground line-through">
              {r.oldState}
            </TableCell>
            <TableCell className="font-medium">{r.newState}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

function Section({
  title,
  count,
  tone,
  children,
}: {
  title: string
  count: number
  tone: "added" | "removed" | "changed" | "unchanged"
  children: React.ReactNode
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-2 space-y-0">
        <span
          className={cn(
            "h-2.5 w-2.5 rounded-full",
            tone === "added" && "bg-green-500",
            tone === "removed" && "bg-red-500",
            tone === "changed" && "bg-amber-500",
            tone === "unchanged" && "bg-muted-foreground/40",
          )}
        />
        <CardTitle className="text-base">
          {title}{" "}
          <span className="font-mono text-sm text-muted-foreground">
            ({count})
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {count === 0 ? (
          <p className="text-sm text-muted-foreground">None.</p>
        ) : (
          <div className="rounded-md border">{children}</div>
        )}
      </CardContent>
    </Card>
  )
}

export function ScanDiffPage() {
  const [target, setTarget] = useState("")
  const [from, setFrom] = useState(AUTO)
  const [to, setTo] = useState(AUTO)

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

  const { data: targetScans } = useScans({
    pageNumber: 1,
    pageSize: 25,
    orderBy: "createdAt",
    orderDirection: "desc",
    target: target || undefined,
  })

  const completed = useMemo(
    () =>
      (targetScans?.items ?? []).filter(
        (s) => s.target === target && s.status === "Completed",
      ),
    [targetScans, target],
  )

  const {
    data: diff,
    isLoading,
    isError,
    error,
  } = useScanDiff(
    target,
    from === AUTO ? undefined : from,
    to === AUTO ? undefined : to,
  )

  const selectTarget = (value: string) => {
    setTarget(value)
    setFrom(AUTO)
    setTo(AUTO)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Scan diff</h1>
        <p className="text-sm text-muted-foreground">
          Compare two completed scans of the same target to see what changed.
        </p>
      </div>

      <div className="flex flex-wrap gap-4">
        <div className="space-y-1.5">
          <Label>Target</Label>
          <Select value={target} onValueChange={selectTarget}>
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

        <div className="space-y-1.5">
          <Label>From</Label>
          <Select value={from} onValueChange={setFrom} disabled={!target}>
            <SelectTrigger className="w-[260px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={AUTO}>Previous (auto)</SelectItem>
              {completed.map((s) => (
                <SelectItem key={s.id} value={s.id}>
                  {formatDate(s.createdAt)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label>To</Label>
          <Select value={to} onValueChange={setTo} disabled={!target}>
            <SelectTrigger className="w-[260px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={AUTO}>Latest (auto)</SelectItem>
              {completed.map((s) => (
                <SelectItem key={s.id} value={s.id}>
                  {formatDate(s.createdAt)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {!target && (
        <p className="text-sm text-muted-foreground">
          Pick a target above. A target needs at least two completed scans to
          produce a diff.
        </p>
      )}

      {target && isLoading && <Skeleton className="h-40 w-full" />}

      {target && isError && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-destructive">
              {error instanceof ApiError
                ? error.message
                : "Could not compute the diff for this target."}
            </p>
          </CardContent>
        </Card>
      )}

      {target && diff && (
        <div className="space-y-4">
          <Section title="Added ports" count={diff.addedPorts.length} tone="added">
            <ResultTable rows={diff.addedPorts} />
          </Section>
          <Section
            title="Removed ports"
            count={diff.removedPorts.length}
            tone="removed"
          >
            <ResultTable rows={diff.removedPorts} />
          </Section>
          <Section
            title="Changed ports"
            count={diff.changedPorts.length}
            tone="changed"
          >
            <ChangeTable rows={diff.changedPorts} />
          </Section>
          <Section
            title="Unchanged ports"
            count={diff.unchangedPorts.length}
            tone="unchanged"
          >
            <ResultTable rows={diff.unchangedPorts} />
          </Section>
        </div>
      )}
    </div>
  )
}

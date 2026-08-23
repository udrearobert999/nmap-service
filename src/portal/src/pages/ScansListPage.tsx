import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useScans } from "@/api/scans"
import { StatusBadge } from "@/components/StatusBadge"
import { NewScanDialog } from "@/components/NewScanDialog"
import { formatDate } from "@/lib/format"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

export function ScansListPage() {
  const navigate = useNavigate()
  const [target, setTarget] = useState("")

  const { data, isLoading, isError, error } = useScans({
    pageNumber: 1,
    pageSize: 25,
    orderBy: "createdAt",
    orderDirection: "desc",
    target: target.trim() || undefined,
  })

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Scans</h1>
          <p className="text-sm text-muted-foreground">
            Live view — updates automatically while scans run.
          </p>
        </div>
        <NewScanDialog />
      </div>

      <Input
        value={target}
        onChange={(e) => setTarget(e.target.value)}
        placeholder="Filter by target…"
        className="max-w-xs"
      />

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Target</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Created</TableHead>
              <TableHead>Created by</TableHead>
              <TableHead className="text-right">Results</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading &&
              Array.from({ length: 4 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell colSpan={5}>
                    <Skeleton className="h-6 w-full" />
                  </TableCell>
                </TableRow>
              ))}

            {isError && (
              <TableRow>
                <TableCell colSpan={5} className="text-center text-destructive">
                  Failed to load scans: {(error as Error)?.message}
                </TableCell>
              </TableRow>
            )}

            {!isLoading && !isError && data?.items.length === 0 && (
              <TableRow>
                <TableCell
                  colSpan={5}
                  className="text-center text-muted-foreground"
                >
                  No scans yet. Start one with “New scan”.
                </TableCell>
              </TableRow>
            )}

            {data?.items.map((scan) => (
              <TableRow
                key={scan.id}
                className="cursor-pointer"
                onClick={() => navigate(`/scans/${scan.id}`)}
              >
                <TableCell className="font-mono">{scan.target}</TableCell>
                <TableCell>
                  <StatusBadge status={scan.status} />
                </TableCell>
                <TableCell>{formatDate(scan.createdAt)}</TableCell>
                <TableCell className="text-muted-foreground">
                  {scan.createdByEmail ?? "—"}
                </TableCell>
                <TableCell className="text-right">
                  {scan.results?.length ?? 0}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {data && (
        <p className="text-xs text-muted-foreground">
          Showing {data.items.length} of {data.total} scan(s).
        </p>
      )}
    </div>
  )
}

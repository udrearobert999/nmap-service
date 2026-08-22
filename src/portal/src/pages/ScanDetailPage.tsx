import { Link, useParams } from "react-router-dom"
import { ArrowLeft } from "lucide-react"
import { useScan } from "@/api/scans"
import { StatusBadge } from "@/components/StatusBadge"
import { RiskAssessmentPanel } from "@/components/RiskAssessmentPanel"
import { formatDate } from "@/lib/format"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

export function ScanDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: scan, isLoading, isError, error } = useScan(id)

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild>
        <Link to="/">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to scans
        </Link>
      </Button>

      {isLoading && <Skeleton className="h-40 w-full" />}

      {isError && (
        <p className="text-destructive">
          Failed to load scan: {(error as Error)?.message}
        </p>
      )}

      {scan && (
        <>
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-3">
                <span className="font-mono">{scan.target}</span>
                <StatusBadge status={scan.status} />
              </CardTitle>
            </CardHeader>
            <CardContent>
              <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2 text-sm">
                <dt className="text-muted-foreground">Scan id</dt>
                <dd className="font-mono">{scan.id}</dd>
                <dt className="text-muted-foreground">Created</dt>
                <dd>{formatDate(scan.createdAt)}</dd>
                <dt className="text-muted-foreground">Completed</dt>
                <dd>{formatDate(scan.completedAt)}</dd>
                {scan.errorMessage && (
                  <>
                    <dt className="text-muted-foreground">Error</dt>
                    <dd className="text-destructive">{scan.errorMessage}</dd>
                  </>
                )}
              </dl>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Results</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Port</TableHead>
                      <TableHead>Protocol</TableHead>
                      <TableHead>Service</TableHead>
                      <TableHead>State</TableHead>
                      <TableHead>Product</TableHead>
                      <TableHead>Version</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {scan.results.length === 0 && (
                      <TableRow>
                        <TableCell
                          colSpan={6}
                          className="text-center text-muted-foreground"
                        >
                          {scan.status === "Completed"
                            ? "No open ports found."
                            : "Results appear once the scan completes."}
                        </TableCell>
                      </TableRow>
                    )}
                    {scan.results.map((r, i) => (
                      <TableRow key={`${r.port}-${r.protocol}-${i}`}>
                        <TableCell className="font-mono">{r.port}</TableCell>
                        <TableCell>{r.protocol}</TableCell>
                        <TableCell>{r.service}</TableCell>
                        <TableCell>{r.state}</TableCell>
                        <TableCell>{r.product ?? "—"}</TableCell>
                        <TableCell>{r.version ?? "—"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </CardContent>
          </Card>

          {scan.status === "Completed" && scan.results.length > 0 && (
            <RiskAssessmentPanel scanId={scan.id} />
          )}
        </>
      )}
    </div>
  )
}

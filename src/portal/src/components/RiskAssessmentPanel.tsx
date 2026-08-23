import { useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ShieldAlert } from "lucide-react"
import {
  useCreateRiskAssessment,
  useLatestRiskAssessment,
} from "@/api/riskAssessments"
import { ApiError } from "@/api/client"
import { StatusBadge } from "@/components/StatusBadge"
import { formatDate } from "@/lib/format"
import { cn } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
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
import type { Finding } from "@/api/types"

function cvssClass(score: number): string {
  if (score >= 7) return "bg-red-600 text-white hover:bg-red-600 border-transparent"
  if (score >= 4)
    return "bg-amber-500 text-white hover:bg-amber-500 border-transparent"
  return "bg-green-600 text-white hover:bg-green-600 border-transparent"
}

function FindingsTable({ findings }: { findings: Finding[] }) {
  if (findings.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No findings — no known vulnerabilities matched this scan's services.
      </p>
    )
  }
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Port</TableHead>
            <TableHead>Service</TableHead>
            <TableHead>Product / Version</TableHead>
            <TableHead>CPE</TableHead>
            <TableHead>CVSS</TableHead>
            <TableHead>KEV</TableHead>
            <TableHead>Confidence</TableHead>
            <TableHead>CVEs</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {findings.map((f, i) => (
            <TableRow key={`${f.port}-${f.service}-${i}`}>
              <TableCell className="font-mono">{f.port}</TableCell>
              <TableCell>{f.service}</TableCell>
              <TableCell>
                {f.product ?? "—"}
                {f.version ? ` ${f.version}` : ""}
              </TableCell>
              <TableCell className="font-mono text-xs">{f.cpe ?? "—"}</TableCell>
              <TableCell>
                <Badge variant="outline" className={cn(cvssClass(f.cvssScore))}>
                  {f.cvssScore.toFixed(1)}
                </Badge>
              </TableCell>
              <TableCell>
                {f.kevFlag ? (
                  <Badge
                    variant="outline"
                    className="bg-red-700 text-white hover:bg-red-700 border-transparent"
                  >
                    KEV
                  </Badge>
                ) : (
                  <span className="text-muted-foreground">—</span>
                )}
              </TableCell>
              <TableCell>{Math.round(f.matchConfidence * 100)}%</TableCell>
              <TableCell>
                {f.matchedCves.length === 0 ? (
                  <span className="text-muted-foreground">—</span>
                ) : (
                  <div className="flex flex-wrap gap-1">
                    {f.matchedCves.map((cve) => (
                      <span
                        key={cve}
                        className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
                      >
                        {cve}
                      </span>
                    ))}
                  </div>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

export function RiskAssessmentPanel({ scanId }: { scanId: string }) {
  const queryClient = useQueryClient()
  const create = useCreateRiskAssessment()
  const { data: assessment, isLoading } = useLatestRiskAssessment(scanId)

  function request() {
    create.mutate(scanId, {
      onSuccess: () => {
        toast.success("Risk assessment requested")
        queryClient.invalidateQueries({
          queryKey: ["risk-assessment-latest", scanId],
        })
      },
      onError: (err) => {
        const msg =
          err instanceof ApiError ? err.message : "Failed to request assessment"
        toast.error("Could not request assessment", { description: msg })
      },
    })
  }

  const isBusy =
    assessment?.status === "Pending" || assessment?.status === "Running"

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ShieldAlert className="h-5 w-5 text-primary" />
          Risk assessment
        </CardTitle>
        <CardDescription>
          Run an automated risk assessment over this scan's results.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <Button onClick={request} disabled={create.isPending || isBusy}>
            {create.isPending
              ? "Requesting…"
              : assessment
                ? "Re-run assessment"
                : "Request risk assessment"}
          </Button>
        </div>

        {isLoading && !assessment && (
          <p className="text-sm text-muted-foreground">Loading…</p>
        )}

        {assessment && (
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-x-8 gap-y-2">
              <div className="flex items-center gap-2">
                <span className="text-sm text-muted-foreground">Status</span>
                <StatusBadge status={assessment.status} />
              </div>
              <div className="flex items-baseline gap-2">
                <span className="text-sm text-muted-foreground">
                  Overall risk score
                </span>
                <span className="font-mono text-2xl font-semibold">
                  {assessment.overallRiskScore ?? "—"}
                </span>
              </div>
            </div>

            <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2 text-sm">
              <dt className="text-muted-foreground">Requested</dt>
              <dd>{formatDate(assessment.requestedAt)}</dd>
              <dt className="text-muted-foreground">Completed</dt>
              <dd>{formatDate(assessment.completedAt)}</dd>
              {assessment.createdByEmail && (
                <>
                  <dt className="text-muted-foreground">Requested by</dt>
                  <dd>{assessment.createdByEmail}</dd>
                </>
              )}
              {assessment.errorMessage && (
                <>
                  <dt className="text-muted-foreground">Error</dt>
                  <dd className="text-destructive">{assessment.errorMessage}</dd>
                </>
              )}
            </dl>

            {isBusy && (
              <p className="text-xs text-muted-foreground">Polling for updates…</p>
            )}

            {assessment.status === "Completed" && (
              <div className="space-y-2">
                <h3 className="text-sm font-semibold">
                  Findings ({assessment.findings.length})
                </h3>
                <FindingsTable findings={assessment.findings} />
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

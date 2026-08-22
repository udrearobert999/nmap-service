import { useState } from "react"
import { toast } from "sonner"
import { ShieldAlert } from "lucide-react"
import {
  useCreateRiskAssessment,
  useRiskAssessment,
} from "@/api/riskAssessments"
import { ApiError } from "@/api/client"
import { StatusBadge } from "@/components/StatusBadge"
import { formatDate } from "@/lib/format"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

export function RiskAssessmentPanel({ scanId }: { scanId: string }) {
  const [assessmentId, setAssessmentId] = useState<string | null>(null)
  const [manualId, setManualId] = useState("")
  const create = useCreateRiskAssessment()
  const { data: assessment, isLoading } = useRiskAssessment(
    assessmentId ?? undefined,
  )

  function request() {
    create.mutate(scanId, {
      onSuccess: ({ id }) => {
        if (id) {
          setAssessmentId(id)
          toast.success("Risk assessment requested")
        } else {
          // Contract returns 202 with no id/Location — ask the user to track it.
          toast.info("Assessment requested", {
            description:
              "The API did not return an id. Paste the assessment id to track it.",
          })
        }
      },
      onError: (err) => {
        const msg =
          err instanceof ApiError ? err.message : "Failed to request assessment"
        toast.error("Could not request assessment", { description: msg })
      },
    })
  }

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
        {!assessmentId && (
          <div className="space-y-3">
            <Button onClick={request} disabled={create.isPending}>
              {create.isPending ? "Requesting…" : "Request risk assessment"}
            </Button>

            {create.isSuccess && (
              <div className="flex flex-wrap items-end gap-2">
                <div className="grid gap-1">
                  <Label htmlFor="assessment-id" className="text-xs">
                    Track by assessment id
                  </Label>
                  <Input
                    id="assessment-id"
                    className="w-[320px] font-mono"
                    placeholder="assessment guid"
                    value={manualId}
                    onChange={(e) => setManualId(e.target.value)}
                  />
                </div>
                <Button
                  variant="secondary"
                  disabled={!manualId.trim()}
                  onClick={() => setAssessmentId(manualId.trim())}
                >
                  Track
                </Button>
              </div>
            )}
          </div>
        )}

        {assessmentId && (
          <div className="space-y-3">
            {isLoading && !assessment && (
              <p className="text-sm text-muted-foreground">Loading…</p>
            )}
            {assessment && (
              <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2 text-sm">
                <dt className="text-muted-foreground">Status</dt>
                <dd>
                  <StatusBadge status={assessment.status} />
                </dd>
                <dt className="text-muted-foreground">Overall risk score</dt>
                <dd className="font-mono text-lg font-semibold">
                  {assessment.overallRiskScore ?? "—"}
                </dd>
                <dt className="text-muted-foreground">Requested</dt>
                <dd>{formatDate(assessment.requestedAt)}</dd>
                <dt className="text-muted-foreground">Completed</dt>
                <dd>{formatDate(assessment.completedAt)}</dd>
                {assessment.errorMessage && (
                  <>
                    <dt className="text-muted-foreground">Error</dt>
                    <dd className="text-destructive">
                      {assessment.errorMessage}
                    </dd>
                  </>
                )}
              </dl>
            )}
            {assessment &&
              (assessment.status === "Pending" ||
                assessment.status === "Running") && (
                <p className="text-xs text-muted-foreground">
                  Polling for updates…
                </p>
              )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

import { useQuery } from "@tanstack/react-query"
import { apiFetch } from "./client"
import type { ScanDiff } from "./types"

export function getScanDiff(
  target: string,
  from?: string,
  to?: string,
): Promise<ScanDiff> {
  const q = new URLSearchParams()
  if (from) q.set("from", from)
  if (to) q.set("to", to)
  const qs = q.toString()

  return apiFetch<ScanDiff>(
    `/scans/diff/${encodeURIComponent(target)}${qs ? `?${qs}` : ""}`,
  )
}

export function useScanDiff(target: string, from?: string, to?: string) {
  return useQuery({
    queryKey: ["scans", "diff", target, from ?? null, to ?? null],
    queryFn: () => getScanDiff(target, from, to),
    enabled: target.trim().length > 0,
    retry: false,
  })
}

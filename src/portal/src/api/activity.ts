import { useQuery } from "@tanstack/react-query"
import { apiFetch } from "./client"
import type { ActivityPoint } from "./types"

export type Bucket = "day" | "hour"

export interface ActivityRange {
  from: string
  to: string
  bucket: Bucket
}

export function getActivity(range: ActivityRange): Promise<ActivityPoint[]> {
  const q = new URLSearchParams({
    from: range.from,
    to: range.to,
    bucket: range.bucket,
  })
  return apiFetch<ActivityPoint[]>(`/dashboard/activity?${q.toString()}`)
}

export function useActivity(range: ActivityRange) {
  return useQuery({
    queryKey: ["dashboard", "activity", range.from, range.to, range.bucket],
    queryFn: () => getActivity(range),
    refetchInterval: 15_000,
  })
}

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { apiFetch } from "./client"
import type { ListScansParams, Paged, Scan } from "./types"

function buildScansQuery(params: ListScansParams): string {
  const q = new URLSearchParams()
  q.set("pageNumber", String(params.pageNumber))
  q.set("pageSize", String(params.pageSize))
  if (params.orderBy) q.set("orderBy", params.orderBy)
  if (params.orderDirection) q.set("orderDirection", params.orderDirection)
  if (params.target) q.set("target", params.target)
  return q.toString()
}

export function listScans(params: ListScansParams): Promise<Paged<Scan>> {
  return apiFetch<Paged<Scan>>(`/scans?${buildScansQuery(params)}`)
}

export function getScan(id: string): Promise<Scan> {
  return apiFetch<Scan>(`/scans/${id}`)
}

export function createScan(target: string): Promise<void> {
  return apiFetch<void>(`/scans`, {
    method: "POST",
    body: { target },
    idempotent: true,
  })
}

export function useScans(params: ListScansParams) {
  return useQuery({
    queryKey: ["scans", params],
    queryFn: () => listScans(params),
    refetchInterval: (query) => {
      const data = query.state.data
      if (!data) return 5000
      const busy = data.items.some(
        (s) => s.status === "Pending" || s.status === "Running",
      )
      return busy ? 3000 : 8000
    },
  })
}

export function useScan(id: string | undefined) {
  return useQuery({
    queryKey: ["scan", id],
    queryFn: () => getScan(id as string),
    enabled: !!id,
    refetchInterval: (query) => {
      const data = query.state.data
      if (!data) return 3000
      return data.status === "Pending" || data.status === "Running" ? 3000 : false
    },
  })
}

export function useCreateScan() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (target: string) => createScan(target),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["scans"] })
    },
  })
}

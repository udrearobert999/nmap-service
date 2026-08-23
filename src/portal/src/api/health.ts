import { useQuery } from "@tanstack/react-query"
import { apiFetch } from "./client"
import type { Health } from "./types"

export function getHealth(): Promise<Health> {
  return apiFetch<Health>("/health")
}

export function useHealth() {
  return useQuery({
    queryKey: ["health"],
    queryFn: getHealth,
    refetchInterval: 10_000,
    retry: false,
    staleTime: 5_000,
  })
}

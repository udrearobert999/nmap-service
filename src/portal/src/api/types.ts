export type ScanStatus = "Pending" | "Running" | "Completed" | "Failed"
export type RiskStatus = "Pending" | "Running" | "Completed" | "Failed"

export interface ScanResult {
  port: number
  protocol: string
  service: string
  state: string
  product: string | null
  version: string | null
}

export interface Scan {
  id: string
  target: string
  status: ScanStatus
  createdAt: string
  completedAt: string | null
  errorMessage: string | null
  createdByEmail: string | null
  results: ScanResult[]
}

export interface Finding {
  port: number
  service: string
  product: string | null
  version: string | null
  cpe: string | null
  matchedCves: string[]
  cvssScore: number
  kevFlag: boolean
  matchConfidence: number
}

export interface RiskAssessment {
  id: string
  scanId: string
  status: RiskStatus
  requestedAt: string
  completedAt: string | null
  errorMessage: string | null
  overallRiskScore: number | null
  createdByEmail: string | null
  findings: Finding[]
}

export type HealthState = "up" | "down"

export interface Health {
  database: HealthState
  services: {
    scanning: HealthState
    assessment: HealthState
  }
}

export interface Paged<T> {
  items: T[]
  total: number
}

export interface ListScansParams {
  pageNumber: number
  pageSize: number
  orderBy?: string
  orderDirection?: "asc" | "desc"
  target?: string
}

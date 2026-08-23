package domain

import "context"

type ScanResult struct {
	Port    int    `json:"port"`
	Service string `json:"service"`
	Product string `json:"product"`
	Version string `json:"version"`
}

type ScanRiskAssessmentRequested struct {
	ScanRiskAssessmentID string       `json:"scanRiskAssessmentId"`
	ScanID               string       `json:"scanId"`
	TeamID               string       `json:"teamId"`
	Results              []ScanResult `json:"results"`
}

type ResolvedCPE struct {
	URI        string
	Confidence float64
}

type CVE struct {
	ID   string
	CVSS float64
}

type Finding struct {
	Port            int
	Service         string
	Product         string
	Version         string
	CPE             string
	MatchedCVEs     []string
	CVSSScore       float64
	KEVFlag         bool
	MatchConfidence float64
}

type CPEResolver interface {
	Resolve(product, version string) ResolvedCPE
}

type CVELookup interface {
	LookupByCPE(ctx context.Context, cpeURI string) ([]CVE, error)
}

type KEVCatalog interface {
	IsKEV(cveID string) bool
}

type Scorer interface {
	ScoreFinding(finding Finding) float64
	Overall(findingScores []float64) float64
}

type StatusWriter interface {
	MarkCompleted(ctx context.Context, scanRiskAssessmentID string, overallRiskScore float64) error
	MarkFailed(ctx context.Context, scanRiskAssessmentID, errorMessage string) error
}

type FindingsWriter interface {
	SaveFindings(ctx context.Context, scanRiskAssessmentID string, findings []Finding) error
}

type HeartbeatWriter interface {
	UpsertHeartbeat(ctx context.Context, serviceName string) error
}

type HealthChecker interface {
	Ping(ctx context.Context) error
}

type ScanRiskAssessmentService interface {
	Assess(ctx context.Context, event ScanRiskAssessmentRequested) error
}

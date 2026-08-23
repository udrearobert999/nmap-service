package application

import (
	"context"
	"log/slog"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type scanRiskAssessmentService struct {
	cpeResolver    domain.CPEResolver
	cveLookup      domain.CVELookup
	kevCatalog     domain.KEVCatalog
	scorer         domain.Scorer
	statusWriter   domain.StatusWriter
	findingsWriter domain.FindingsWriter
}

func NewScanRiskAssessmentService(
	cpeResolver domain.CPEResolver,
	cveLookup domain.CVELookup,
	kevCatalog domain.KEVCatalog,
	scorer domain.Scorer,
	statusWriter domain.StatusWriter,
	findingsWriter domain.FindingsWriter,
) domain.ScanRiskAssessmentService {
	return &scanRiskAssessmentService{
		cpeResolver:    cpeResolver,
		cveLookup:      cveLookup,
		kevCatalog:     kevCatalog,
		scorer:         scorer,
		statusWriter:   statusWriter,
		findingsWriter: findingsWriter,
	}
}

func (s *scanRiskAssessmentService) Assess(ctx context.Context, event domain.ScanRiskAssessmentRequested) error {
	slog.Info("assessing scan risk",
		"scanRiskAssessmentId", event.ScanRiskAssessmentID,
		"scanId", event.ScanID,
		"results", len(event.Results))

	findings := make([]domain.Finding, 0, len(event.Results))
	scores := make([]float64, 0, len(event.Results))

	for _, result := range event.Results {
		finding := s.assessResult(ctx, result)
		findings = append(findings, finding)
		scores = append(scores, s.scorer.ScoreFinding(finding))
	}

	overall := s.scorer.Overall(scores)

	if err := s.findingsWriter.SaveFindings(ctx, event.ScanRiskAssessmentID, findings); err != nil {
		_ = s.statusWriter.MarkFailed(ctx, event.ScanRiskAssessmentID, err.Error())
		return err
	}

	if err := s.statusWriter.MarkCompleted(ctx, event.ScanRiskAssessmentID, overall); err != nil {
		return err
	}

	slog.Info("completed scan risk assessment",
		"scanRiskAssessmentId", event.ScanRiskAssessmentID,
		"overallRiskScore", overall,
		"findings", len(findings))

	return nil
}

func (s *scanRiskAssessmentService) assessResult(ctx context.Context, result domain.ScanResult) domain.Finding {
	resolved := s.cpeResolver.Resolve(result.Product, result.Version)

	var cves []domain.CVE
	if resolved.URI != "" {
		matches, err := s.cveLookup.LookupByCPE(ctx, resolved.URI)
		if err != nil {
			slog.Warn("cve lookup failed", "cpe", resolved.URI, "error", err)
		}
		cves = matches
	}

	matchedIDs := make([]string, 0, len(cves))
	maxCvss := 0.0
	kevFlag := false
	for _, cve := range cves {
		matchedIDs = append(matchedIDs, cve.ID)
		if cve.CVSS > maxCvss {
			maxCvss = cve.CVSS
		}
		if s.kevCatalog.IsKEV(cve.ID) {
			kevFlag = true
		}
	}

	return domain.Finding{
		Port:            result.Port,
		Service:         result.Service,
		Product:         result.Product,
		Version:         result.Version,
		CPE:             resolved.URI,
		MatchedCVEs:     matchedIDs,
		CVSSScore:       maxCvss,
		KEVFlag:         kevFlag,
		MatchConfidence: resolved.Confidence,
	}
}

package postgres

import (
	"context"
	"fmt"
	"strings"
	"time"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

func (r *repository) GetCachedCVEs(ctx context.Context, cpeURI string, freshAfter time.Time) ([]domain.CVE, error) {
	const sql = `
		SELECT "CveId", "CvssScore"
		FROM "CveCacheEntries"
		WHERE "CpeUri" = $1 AND "CachedAt" > $2`

	rows, err := r.pool.Query(ctx, sql, cpeURI, freshAfter)
	if err != nil {
		return nil, fmt.Errorf("querying cve cache: %w", err)
	}
	defer rows.Close()

	var cves []domain.CVE
	for rows.Next() {
		var cve domain.CVE
		if err := rows.Scan(&cve.ID, &cve.CVSS); err != nil {
			return nil, fmt.Errorf("scanning cve cache row: %w", err)
		}
		cves = append(cves, cve)
	}

	return cves, rows.Err()
}

func (r *repository) PutCachedCVEs(ctx context.Context, cpeURI string, cves []domain.CVE) error {
	if len(cves) == 0 {
		return nil
	}

	const sql = `
		INSERT INTO "CveCacheEntries" ("CpeUri", "CveId", "CvssScore", "CachedAt")
		VALUES ($1, $2, $3, now())
		ON CONFLICT ("CpeUri", "CveId") DO UPDATE
		SET "CvssScore" = EXCLUDED."CvssScore", "CachedAt" = now()`

	for _, cve := range cves {
		if _, err := r.pool.Exec(ctx, sql, cpeURI, cve.ID, cve.CVSS); err != nil {
			return fmt.Errorf("caching cve %s: %w", cve.ID, err)
		}
	}

	return nil
}

func (r *repository) SaveFindings(ctx context.Context, scanRiskAssessmentID string, findings []domain.Finding) error {
	tx, err := r.pool.Begin(ctx)
	if err != nil {
		return fmt.Errorf("beginning findings transaction: %w", err)
	}
	defer tx.Rollback(ctx)

	if _, err := tx.Exec(ctx,
		`DELETE FROM "ScanRiskAssessmentFindings" WHERE "ScanRiskAssessmentId" = $1::uuid`,
		scanRiskAssessmentID); err != nil {
		return fmt.Errorf("clearing existing findings: %w", err)
	}

	const insert = `
		INSERT INTO "ScanRiskAssessmentFindings"
			("Id", "ScanRiskAssessmentId", "Port", "Service", "Product", "Version", "Cpe",
			 "MatchedCves", "CvssScore", "KevFlag", "MatchConfidence")
		VALUES (gen_random_uuid(), $1::uuid, $2, $3, $4, $5, $6, $7, $8, $9, $10)`

	for _, f := range findings {
		if _, err := tx.Exec(ctx, insert,
			scanRiskAssessmentID,
			f.Port,
			f.Service,
			nullable(f.Product),
			nullable(f.Version),
			nullable(f.CPE),
			strings.Join(f.MatchedCVEs, ","),
			f.CVSSScore,
			f.KEVFlag,
			f.MatchConfidence,
		); err != nil {
			return fmt.Errorf("inserting finding: %w", err)
		}
	}

	return tx.Commit(ctx)
}

func nullable(value string) *string {
	if strings.TrimSpace(value) == "" {
		return nil
	}
	return &value
}

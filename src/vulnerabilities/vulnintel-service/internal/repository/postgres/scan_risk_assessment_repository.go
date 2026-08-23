package postgres

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type repository struct {
	pool *pgxpool.Pool
}

var (
	_ domain.StatusWriter    = (*repository)(nil)
	_ domain.HeartbeatWriter = (*repository)(nil)
	_ domain.HealthChecker   = (*repository)(nil)
	_ domain.FindingsWriter  = (*repository)(nil)
)

func NewRepository(pool *pgxpool.Pool) *repository {
	return &repository{pool: pool}
}

func (r *repository) MarkCompleted(
	ctx context.Context,
	scanRiskAssessmentID string,
	overallRiskScore float64,
) error {
	const sql = `
		UPDATE "ScanRiskAssessments"
		SET "Status" = 'Completed',
		    "OverallRiskScore" = $1,
		    "CompletedAt" = now()
		WHERE "Id" = $2::uuid`

	if _, err := r.pool.Exec(ctx, sql, overallRiskScore, scanRiskAssessmentID); err != nil {
		return fmt.Errorf("marking scan risk assessment %s completed: %w", scanRiskAssessmentID, err)
	}

	return nil
}

func (r *repository) MarkFailed(
	ctx context.Context,
	scanRiskAssessmentID, errorMessage string,
) error {
	const sql = `
		UPDATE "ScanRiskAssessments"
		SET "Status" = 'Failed',
		    "ErrorMessage" = $1
		WHERE "Id" = $2::uuid`

	if _, err := r.pool.Exec(ctx, sql, errorMessage, scanRiskAssessmentID); err != nil {
		return fmt.Errorf("marking scan risk assessment %s failed: %w", scanRiskAssessmentID, err)
	}

	return nil
}

func (r *repository) UpsertHeartbeat(ctx context.Context, serviceName string) error {
	const sql = `
		INSERT INTO "ServiceHeartbeats" ("ServiceName", "LastSeenAt")
		VALUES ($1, now())
		ON CONFLICT ("ServiceName") DO UPDATE SET "LastSeenAt" = now()`

	if _, err := r.pool.Exec(ctx, sql, serviceName); err != nil {
		return fmt.Errorf("upserting heartbeat for %s: %w", serviceName, err)
	}

	return nil
}

func (r *repository) Ping(ctx context.Context) error {
	return r.pool.Ping(ctx)
}

package cve

import (
	"context"
	"log/slog"
	"time"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type CacheStore interface {
	GetCachedCVEs(ctx context.Context, cpeURI string, freshAfter time.Time) ([]domain.CVE, error)
	PutCachedCVEs(ctx context.Context, cpeURI string, cves []domain.CVE) error
}

type NVDClient interface {
	LookupByCPE(ctx context.Context, cpeURI string) ([]domain.CVE, error)
}

type CachedLookup struct {
	store CacheStore
	nvd   NVDClient
	ttl   time.Duration
}

var _ domain.CVELookup = (*CachedLookup)(nil)

func NewCachedLookup(store CacheStore, nvd NVDClient, ttl time.Duration) *CachedLookup {
	return &CachedLookup{store: store, nvd: nvd, ttl: ttl}
}

func (l *CachedLookup) LookupByCPE(ctx context.Context, cpeURI string) ([]domain.CVE, error) {
	if cpeURI == "" {
		return nil, nil
	}

	cached, err := l.store.GetCachedCVEs(ctx, cpeURI, time.Now().Add(-l.ttl))
	if err != nil {
		slog.Warn("cve cache read failed", "cpe", cpeURI, "error", err)
	} else if len(cached) > 0 {
		return cached, nil
	}

	fresh, err := l.nvd.LookupByCPE(ctx, cpeURI)
	if err != nil {
		slog.Warn("nvd lookup failed, continuing without CVEs", "cpe", cpeURI, "error", err)
		return cached, nil
	}

	if len(fresh) > 0 {
		if err := l.store.PutCachedCVEs(ctx, cpeURI, fresh); err != nil {
			slog.Warn("cve cache write failed", "cpe", cpeURI, "error", err)
		}
	}

	return fresh, nil
}

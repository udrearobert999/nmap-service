package kev

import (
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"net/http"
	"sync"
	"time"
)

const feedURL = "https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json"

type Catalog struct {
	httpClient *http.Client

	mu  sync.RWMutex
	set map[string]struct{}
}

func NewCatalog() *Catalog {
	return &Catalog{
		httpClient: &http.Client{Timeout: 30 * time.Second},
		set:        make(map[string]struct{}),
	}
}

func (c *Catalog) IsKEV(cveID string) bool {
	c.mu.RLock()
	defer c.mu.RUnlock()
	_, ok := c.set[cveID]
	return ok
}

func (c *Catalog) Size() int {
	c.mu.RLock()
	defer c.mu.RUnlock()
	return len(c.set)
}

func (c *Catalog) Refresh(ctx context.Context) error {
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, feedURL, nil)
	if err != nil {
		return fmt.Errorf("building kev request: %w", err)
	}

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return fmt.Errorf("downloading kev feed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("kev feed returned status %d", resp.StatusCode)
	}

	var feed struct {
		Vulnerabilities []struct {
			CveID string `json:"cveID"`
		} `json:"vulnerabilities"`
	}
	if err := json.NewDecoder(resp.Body).Decode(&feed); err != nil {
		return fmt.Errorf("decoding kev feed: %w", err)
	}

	set := make(map[string]struct{}, len(feed.Vulnerabilities))
	for _, v := range feed.Vulnerabilities {
		set[v.CveID] = struct{}{}
	}

	c.mu.Lock()
	c.set = set
	c.mu.Unlock()

	return nil
}

func (c *Catalog) RunRefresh(ctx context.Context, interval time.Duration) {
	ticker := time.NewTicker(interval)
	defer ticker.Stop()

	for {
		if err := c.Refresh(ctx); err != nil {
			slog.Error("failed to refresh KEV catalog", "error", err)
		} else {
			slog.Info("refreshed KEV catalog", "entries", c.Size())
		}

		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
		}
	}
}

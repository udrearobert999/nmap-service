package nvd

import (
	"context"
	"net/http"
	"net/http/httptest"
	"sync/atomic"
	"testing"
	"time"
)

const sampleBody = `{"vulnerabilities":[
  {"cve":{"id":"CVE-2020-0001","metrics":{"cvssMetricV31":[{"cvssData":{"baseScore":7.5}}]}}},
  {"cve":{"id":"CVE-2020-0002","metrics":{"cvssMetricV31":[{"cvssData":{"baseScore":9.8}}]}}}
]}`

func testClient(t *testing.T, handler http.HandlerFunc) *Client {
	t.Helper()

	server := httptest.NewServer(handler)
	t.Cleanup(server.Close)

	return NewClient(
		WithBaseURL(server.URL),
		WithHTTPClient(server.Client()),
		WithMinInterval(0),
	)
}

func TestLookupSortsAndReturnsCVEs(t *testing.T) {
	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		w.Write([]byte(sampleBody))
	})

	cves, err := client.LookupByCPE(context.Background(), "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*")
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if len(cves) != 2 {
		t.Fatalf("expected 2 CVEs, got %d", len(cves))
	}

	if cves[0].ID != "CVE-2020-0002" {
		t.Errorf("expected highest CVSS first, got %s (%.1f)", cves[0].ID, cves[0].CVSS)
	}
}

func TestLookupRetriesOnRateLimit(t *testing.T) {
	var calls int32

	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		if atomic.AddInt32(&calls, 1) == 1 {
			w.Header().Set("Retry-After", "0")
			w.WriteHeader(http.StatusTooManyRequests)
			return
		}
		w.Write([]byte(sampleBody))
	})

	cves, err := client.LookupByCPE(context.Background(), "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*")
	if err != nil {
		t.Fatalf("expected retry to succeed, got %v", err)
	}

	if got := atomic.LoadInt32(&calls); got != 2 {
		t.Errorf("expected 2 calls, got %d", got)
	}

	if len(cves) != 2 {
		t.Errorf("expected 2 CVEs after retry, got %d", len(cves))
	}
}

func TestLookupDoesNotRetryClientErrors(t *testing.T) {
	var calls int32

	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		atomic.AddInt32(&calls, 1)
		w.WriteHeader(http.StatusBadRequest)
	})

	if _, err := client.LookupByCPE(context.Background(), "bad-cpe"); err == nil {
		t.Fatal("expected an error for a 400 response")
	}

	if got := atomic.LoadInt32(&calls); got != 1 {
		t.Errorf("expected a single call for a non-retryable status, got %d", got)
	}
}

func TestLookupGivesUpAfterMaxAttempts(t *testing.T) {
	var calls int32

	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		atomic.AddInt32(&calls, 1)
		w.Header().Set("Retry-After", "0")
		w.WriteHeader(http.StatusServiceUnavailable)
	})

	if _, err := client.LookupByCPE(context.Background(), "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*"); err == nil {
		t.Fatal("expected an error after exhausting attempts")
	}

	if got := atomic.LoadInt32(&calls); got != maxAttempts {
		t.Errorf("expected %d attempts, got %d", maxAttempts, got)
	}
}

func TestLookupSendsAPIKeyHeader(t *testing.T) {
	seen := make(chan string, 1)

	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		seen <- r.Header.Get("apiKey")
		w.Write([]byte(sampleBody))
	}))
	t.Cleanup(server.Close)

	client := NewClient(
		WithBaseURL(server.URL),
		WithHTTPClient(server.Client()),
		WithAPIKey("secret-key"),
		WithMinInterval(0),
	)

	if _, err := client.LookupByCPE(context.Background(), "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*"); err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if got := <-seen; got != "secret-key" {
		t.Errorf("expected apiKey header to be forwarded, got %q", got)
	}
}

func TestRateLimiterSpacesRequests(t *testing.T) {
	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		w.Write([]byte(sampleBody))
	})
	client.minInterval = 40 * time.Millisecond

	start := time.Now()
	for i := 0; i < 3; i++ {
		if _, err := client.LookupByCPE(context.Background(), "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*"); err != nil {
			t.Fatalf("unexpected error: %v", err)
		}
	}

	if elapsed := time.Since(start); elapsed < 80*time.Millisecond {
		t.Errorf("expected requests to be spaced by the rate limiter, took %s", elapsed)
	}
}

func TestLookupHonoursContextCancellation(t *testing.T) {
	client := testClient(t, func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Retry-After", "60")
		w.WriteHeader(http.StatusTooManyRequests)
	})

	ctx, cancel := context.WithTimeout(context.Background(), 50*time.Millisecond)
	defer cancel()

	if _, err := client.LookupByCPE(ctx, "cpe:2.3:a:x:y:1:*:*:*:*:*:*:*"); err == nil {
		t.Fatal("expected cancellation to abort the retry loop")
	}
}

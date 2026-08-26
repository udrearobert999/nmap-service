package nvd

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"math/rand"
	"net/http"
	"net/url"
	"sort"
	"strconv"
	"sync"
	"time"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

const (
	baseURL        = "https://services.nvd.nist.gov/rest/json/cves/2.0"
	resultsPerPage = 50
	maxCVEs        = 20

	anonymousInterval = 6500 * time.Millisecond
	keyedInterval     = 700 * time.Millisecond

	maxAttempts    = 4
	baseBackoff    = time.Second
	maxBackoff     = 30 * time.Second
	requestTimeout = 20 * time.Second
)

type Option func(*Client)

func WithAPIKey(apiKey string) Option {
	return func(c *Client) {
		c.apiKey = apiKey
		if apiKey != "" {
			c.minInterval = keyedInterval
		}
	}
}

func WithHTTPClient(httpClient *http.Client) Option {
	return func(c *Client) {
		c.httpClient = httpClient
	}
}

func WithBaseURL(rawURL string) Option {
	return func(c *Client) {
		c.baseURL = rawURL
	}
}

func WithMinInterval(interval time.Duration) Option {
	return func(c *Client) {
		c.minInterval = interval
	}
}

type Client struct {
	httpClient  *http.Client
	baseURL     string
	apiKey      string
	minInterval time.Duration

	mu          sync.Mutex
	nextAllowed time.Time
}

func NewClient(options ...Option) *Client {
	client := &Client{
		httpClient:  &http.Client{Timeout: requestTimeout},
		baseURL:     baseURL,
		minInterval: anonymousInterval,
	}

	for _, option := range options {
		option(client)
	}

	return client
}

type apiResponse struct {
	Vulnerabilities []struct {
		CVE struct {
			ID      string `json:"id"`
			Metrics struct {
				CvssV31 []metric `json:"cvssMetricV31"`
				CvssV30 []metric `json:"cvssMetricV30"`
				CvssV2  []metric `json:"cvssMetricV2"`
			} `json:"metrics"`
		} `json:"cve"`
	} `json:"vulnerabilities"`
}

type metric struct {
	CvssData struct {
		BaseScore float64 `json:"baseScore"`
	} `json:"cvssData"`
}

func (c *Client) LookupByCPE(ctx context.Context, cpeURI string) ([]domain.CVE, error) {
	query := url.Values{}
	query.Set("cpeName", cpeURI)
	query.Set("resultsPerPage", strconv.Itoa(resultsPerPage))
	requestURL := c.baseURL + "?" + query.Encode()

	var lastErr error

	for attempt := 0; attempt < maxAttempts; attempt++ {
		if attempt > 0 {
			if err := sleepCtx(ctx, backoffFor(attempt, lastErr)); err != nil {
				return nil, err
			}
		}

		if err := c.awaitSlot(ctx); err != nil {
			return nil, err
		}

		body, err := c.do(ctx, requestURL)
		if err == nil {
			return parse(body)
		}

		lastErr = err
		if !isRetryable(err) {
			return nil, err
		}
	}

	return nil, fmt.Errorf("nvd lookup failed after %d attempts: %w", maxAttempts, lastErr)
}

func (c *Client) do(ctx context.Context, requestURL string) ([]byte, error) {
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, requestURL, nil)
	if err != nil {
		return nil, fmt.Errorf("building nvd request: %w", err)
	}

	if c.apiKey != "" {
		req.Header.Set("apiKey", c.apiKey)
	}

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return nil, transportError{err: err}
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, statusError{
			status:     resp.StatusCode,
			retryAfter: parseRetryAfter(resp.Header.Get("Retry-After")),
		}
	}

	return io.ReadAll(resp.Body)
}

func (c *Client) awaitSlot(ctx context.Context) error {
	c.mu.Lock()
	now := time.Now()
	wait := time.Duration(0)
	if c.nextAllowed.After(now) {
		wait = c.nextAllowed.Sub(now)
	}
	c.nextAllowed = now.Add(wait + c.minInterval)
	c.mu.Unlock()

	return sleepCtx(ctx, wait)
}

func parse(body []byte) ([]domain.CVE, error) {
	var parsed apiResponse
	if err := json.Unmarshal(body, &parsed); err != nil {
		return nil, fmt.Errorf("decoding nvd response: %w", err)
	}

	cves := make([]domain.CVE, 0, len(parsed.Vulnerabilities))
	for _, v := range parsed.Vulnerabilities {
		cves = append(cves, domain.CVE{
			ID:   v.CVE.ID,
			CVSS: baseScore(v.CVE.Metrics.CvssV31, v.CVE.Metrics.CvssV30, v.CVE.Metrics.CvssV2),
		})
	}

	sort.Slice(cves, func(i, j int) bool { return cves[i].CVSS > cves[j].CVSS })
	if len(cves) > maxCVEs {
		cves = cves[:maxCVEs]
	}

	return cves, nil
}

type transportError struct{ err error }

func (e transportError) Error() string { return "calling nvd: " + e.err.Error() }
func (e transportError) Unwrap() error { return e.err }

type statusError struct {
	status     int
	retryAfter time.Duration
}

func (e statusError) Error() string { return fmt.Sprintf("nvd returned status %d", e.status) }

func isRetryable(err error) bool {
	switch typed := err.(type) {
	case transportError:
		return true
	case statusError:
		return typed.status == http.StatusTooManyRequests ||
			typed.status == http.StatusForbidden ||
			typed.status >= http.StatusInternalServerError
	default:
		return false
	}
}

func backoffFor(attempt int, err error) time.Duration {
	if typed, ok := err.(statusError); ok && typed.retryAfter > 0 {
		return capBackoff(typed.retryAfter)
	}

	delay := baseBackoff << (attempt - 1)
	jitter := time.Duration(rand.Int63n(int64(baseBackoff)))

	return capBackoff(delay + jitter)
}

func capBackoff(delay time.Duration) time.Duration {
	if delay > maxBackoff {
		return maxBackoff
	}
	return delay
}

func parseRetryAfter(value string) time.Duration {
	if value == "" {
		return 0
	}

	if seconds, err := strconv.Atoi(value); err == nil && seconds >= 0 {
		return time.Duration(seconds) * time.Second
	}

	if when, err := http.ParseTime(value); err == nil {
		if delay := time.Until(when); delay > 0 {
			return delay
		}
	}

	return 0
}

func sleepCtx(ctx context.Context, delay time.Duration) error {
	if delay <= 0 {
		return ctx.Err()
	}

	timer := time.NewTimer(delay)
	defer timer.Stop()

	select {
	case <-ctx.Done():
		return ctx.Err()
	case <-timer.C:
		return nil
	}
}

func baseScore(sets ...[]metric) float64 {
	for _, set := range sets {
		if len(set) > 0 {
			return set[0].CvssData.BaseScore
		}
	}
	return 0
}

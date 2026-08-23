package nvd

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"sort"
	"time"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

const (
	baseURL        = "https://services.nvd.nist.gov/rest/json/cves/2.0"
	resultsPerPage = 50
	maxCVEs        = 20
)

type Client struct {
	httpClient *http.Client
}

func NewClient() *Client {
	return &Client{httpClient: &http.Client{Timeout: 20 * time.Second}}
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
	query.Set("resultsPerPage", fmt.Sprintf("%d", resultsPerPage))

	req, err := http.NewRequestWithContext(ctx, http.MethodGet, baseURL+"?"+query.Encode(), nil)
	if err != nil {
		return nil, fmt.Errorf("building nvd request: %w", err)
	}

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("calling nvd: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("nvd returned status %d", resp.StatusCode)
	}

	var body apiResponse
	if err := json.NewDecoder(resp.Body).Decode(&body); err != nil {
		return nil, fmt.Errorf("decoding nvd response: %w", err)
	}

	cves := make([]domain.CVE, 0, len(body.Vulnerabilities))
	for _, v := range body.Vulnerabilities {
		cves = append(cves, domain.CVE{ID: v.CVE.ID, CVSS: baseScore(v.CVE.Metrics.CvssV31, v.CVE.Metrics.CvssV30, v.CVE.Metrics.CvssV2)})
	}

	sort.Slice(cves, func(i, j int) bool { return cves[i].CVSS > cves[j].CVSS })
	if len(cves) > maxCVEs {
		cves = cves[:maxCVEs]
	}

	return cves, nil
}

func baseScore(sets ...[]metric) float64 {
	for _, set := range sets {
		if len(set) > 0 {
			return set[0].CvssData.BaseScore
		}
	}
	return 0
}

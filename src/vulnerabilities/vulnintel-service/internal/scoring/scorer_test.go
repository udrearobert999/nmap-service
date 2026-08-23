package scoring

import (
	"testing"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

func TestScoreFinding(t *testing.T) {
	scorer := NewScorer()

	tests := []struct {
		name    string
		finding domain.Finding
		want    float64
	}{
		{
			name:    "cvss passthrough",
			finding: domain.Finding{MatchedCVEs: []string{"CVE-1"}, CVSSScore: 7.5},
			want:    7.5,
		},
		{
			name:    "kev multiplier capped at 10",
			finding: domain.Finding{MatchedCVEs: []string{"CVE-1"}, CVSSScore: 9.5, KEVFlag: true},
			want:    10,
		},
		{
			name:    "heuristic tier for telnet with no cves",
			finding: domain.Finding{Service: "telnet"},
			want:    7.0,
		},
		{
			name:    "no cves and benign service scores zero",
			finding: domain.Finding{Service: "http"},
			want:    0,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := scorer.ScoreFinding(tt.finding); got != tt.want {
				t.Errorf("ScoreFinding = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestOverallWeightsWorst(t *testing.T) {
	scorer := NewScorer()

	if got := scorer.Overall(nil); got != 0 {
		t.Errorf("Overall(nil) = %v, want 0", got)
	}

	got := scorer.Overall([]float64{9.0, 3.0})
	want := round1(0.7*9.0 + 0.3*6.0)
	if got != want {
		t.Errorf("Overall = %v, want %v", got, want)
	}
	if got <= 6.0 {
		t.Errorf("overall %v should be weighted toward the worst finding", got)
	}
}

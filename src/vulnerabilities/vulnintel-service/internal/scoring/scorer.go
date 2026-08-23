package scoring

import (
	"math"
	"strings"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

const kevMultiplier = 1.15

type Scorer struct{}

var _ domain.Scorer = (*Scorer)(nil)

func NewScorer() *Scorer {
	return &Scorer{}
}

func (s *Scorer) ScoreFinding(finding domain.Finding) float64 {
	base := finding.CVSSScore
	if len(finding.MatchedCVEs) == 0 {
		base = heuristicBase(finding.Service)
	}

	score := base
	if finding.KEVFlag {
		score *= kevMultiplier
	}

	return round1(math.Min(score, 10))
}

func (s *Scorer) Overall(findingScores []float64) float64 {
	if len(findingScores) == 0 {
		return 0
	}

	worst := 0.0
	sum := 0.0
	for _, score := range findingScores {
		worst = math.Max(worst, score)
		sum += score
	}

	average := sum / float64(len(findingScores))
	overall := 0.7*worst + 0.3*average

	return round1(math.Min(overall, 10))
}

func heuristicBase(service string) float64 {
	switch strings.ToLower(service) {
	case "telnet":
		return 7.0
	case "ftp", "ftp-data":
		return 5.0
	case "rdp", "ms-wbt-server":
		return 6.0
	case "vnc":
		return 6.0
	case "smb", "microsoft-ds", "netbios-ssn":
		return 5.0
	default:
		return 0.0
	}
}

func round1(value float64) float64 {
	return math.Round(value*10) / 10
}

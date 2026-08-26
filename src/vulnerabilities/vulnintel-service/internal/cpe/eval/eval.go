package eval

import (
	"fmt"
	"strings"

	"github.com/udrearobert999/vulnintel-service/internal/cpe"
)

type Class string

const (
	TruePositive  Class = "TP"
	FalsePositive Class = "FP"
	FalseNegative Class = "FN"
	TrueNegative  Class = "TN"
)

type ResolveFunc func(product, version string) (uri string, confidence float64)

type Outcome struct {
	Sample     Sample
	GotCPE     string
	Confidence float64
	Accepted   bool
	Class      Class
}

type Metrics struct {
	Name      string
	Threshold float64
	TP        int
	FP        int
	FN        int
	TN        int
	Outcomes  []Outcome
}

func (m Metrics) Precision() float64 {
	denominator := m.TP + m.FP
	if denominator == 0 {
		return 0
	}
	return float64(m.TP) / float64(denominator)
}

func (m Metrics) Recall() float64 {
	denominator := m.TP + m.FN
	if denominator == 0 {
		return 0
	}
	return float64(m.TP) / float64(denominator)
}

func (m Metrics) F1() float64 {
	precision, recall := m.Precision(), m.Recall()
	if precision+recall == 0 {
		return 0
	}
	return 2 * precision * recall / (precision + recall)
}

func (m Metrics) Accuracy() float64 {
	total := m.TP + m.FP + m.FN + m.TN
	if total == 0 {
		return 0
	}
	return float64(m.TP+m.TN) / float64(total)
}

func DictionaryResolver() ResolveFunc {
	resolver := cpe.NewResolver()

	return func(product, version string) (string, float64) {
		resolved := resolver.Resolve(product, version)
		return resolved.URI, resolved.Confidence
	}
}

func NaiveResolver() ResolveFunc {
	return func(product, version string) (string, float64) {
		normalizedProduct := strings.ToLower(strings.TrimSpace(product))
		if normalizedProduct == "" {
			return "", 0
		}

		token := strings.ReplaceAll(normalizedProduct, " ", "_")
		token = strings.ReplaceAll(token, "/", "_")

		normalizedVersion := strings.ToLower(strings.TrimSpace(version))
		if normalizedVersion == "" {
			normalizedVersion = "*"
		}
		normalizedVersion = strings.ReplaceAll(normalizedVersion, " ", "_")

		uri := fmt.Sprintf("cpe:2.3:a:%s:%s:%s:*:*:*:*:*:*:*", token, token, normalizedVersion)
		return uri, 1
	}
}

func Evaluate(name string, resolve ResolveFunc, corpus []Sample, threshold float64) Metrics {
	metrics := Metrics{Name: name, Threshold: threshold}

	for _, sample := range corpus {
		uri, confidence := resolve(sample.Product, sample.Version)
		accepted := uri != "" && confidence >= threshold
		identifiable := sample.WantCPE != ""

		outcome := Outcome{
			Sample:     sample,
			GotCPE:     uri,
			Confidence: confidence,
			Accepted:   accepted,
		}

		switch {
		case identifiable && accepted && uri == sample.WantCPE:
			outcome.Class = TruePositive
			metrics.TP++
		case identifiable && accepted:
			outcome.Class = FalsePositive
			metrics.FP++
		case identifiable:
			outcome.Class = FalseNegative
			metrics.FN++
		case accepted:
			outcome.Class = FalsePositive
			metrics.FP++
		default:
			outcome.Class = TrueNegative
			metrics.TN++
		}

		metrics.Outcomes = append(metrics.Outcomes, outcome)
	}

	return metrics
}

func Report(results []Metrics) string {
	var builder strings.Builder

	builder.WriteString("| resolver | threshold | TP | FP | FN | TN | precision | recall | F1 | accuracy |\n")
	builder.WriteString("|---|---|---|---|---|---|---|---|---|---|\n")

	for _, m := range results {
		builder.WriteString(fmt.Sprintf(
			"| %s | %.2f | %d | %d | %d | %d | %.3f | %.3f | %.3f | %.3f |\n",
			m.Name, m.Threshold, m.TP, m.FP, m.FN, m.TN,
			m.Precision(), m.Recall(), m.F1(), m.Accuracy(),
		))
	}

	return builder.String()
}

func Failures(m Metrics) string {
	var builder strings.Builder

	for _, outcome := range m.Outcomes {
		if outcome.Class == TruePositive || outcome.Class == TrueNegative {
			continue
		}

		builder.WriteString(fmt.Sprintf(
			"  [%s] %-38q conf=%.2f\n        want=%s\n        got =%s\n",
			outcome.Class,
			strings.TrimSpace(outcome.Sample.Product+" "+outcome.Sample.Version),
			outcome.Confidence,
			orDash(outcome.Sample.WantCPE),
			orDash(outcome.GotCPE),
		))
	}

	return builder.String()
}

func orDash(value string) string {
	if value == "" {
		return "(none)"
	}
	return value
}

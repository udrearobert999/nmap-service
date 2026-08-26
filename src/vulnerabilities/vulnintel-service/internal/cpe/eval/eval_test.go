package eval

import "testing"

func TestDictionaryResolverBeatsNaiveBaseline(t *testing.T) {
	corpus := Corpus()

	dictionary := Evaluate("dictionary", DictionaryResolver(), corpus, 0)
	naive := Evaluate("naive", NaiveResolver(), corpus, 0)

	if dictionary.F1() <= naive.F1() {
		t.Errorf("dictionary F1 %.3f did not beat naive baseline F1 %.3f",
			dictionary.F1(), naive.F1())
	}

	if dictionary.Precision() <= naive.Precision() {
		t.Errorf("dictionary precision %.3f did not beat naive baseline precision %.3f",
			dictionary.Precision(), naive.Precision())
	}

	if dictionary.Recall() < naive.Recall() {
		t.Errorf("dictionary recall %.3f regressed against naive baseline recall %.3f",
			dictionary.Recall(), naive.Recall())
	}
}

func TestConfidenceThresholdImprovesPrecision(t *testing.T) {
	corpus := Corpus()

	permissive := Evaluate("dictionary", DictionaryResolver(), corpus, 0)
	strict := Evaluate("dictionary", DictionaryResolver(), corpus, 0.9)

	if strict.Precision() <= permissive.Precision() {
		t.Errorf("thresholding at 0.9 did not improve precision: %.3f vs %.3f",
			strict.Precision(), permissive.Precision())
	}
}

func TestEvaluationReport(t *testing.T) {
	corpus := Corpus()

	results := []Metrics{
		Evaluate("naive", NaiveResolver(), corpus, 0),
		Evaluate("dictionary", DictionaryResolver(), corpus, 0),
		Evaluate("dictionary", DictionaryResolver(), corpus, 0.5),
		Evaluate("dictionary", DictionaryResolver(), corpus, 0.9),
	}

	t.Logf("\nCPE resolution evaluation (n=%d)\n\n%s", len(corpus), Report(results))

	for _, m := range results {
		if failures := Failures(m); failures != "" {
			t.Logf("\n%s @ %.2f failures:\n%s", m.Name, m.Threshold, failures)
		}
	}
}

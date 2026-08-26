package main

import (
	"flag"
	"fmt"
	"os"

	"github.com/udrearobert999/vulnintel-service/internal/cpe/eval"
)

func main() {
	showFailures := flag.Bool("failures", false, "list every misclassified sample")
	flag.Parse()

	corpus := eval.Corpus()

	results := []eval.Metrics{
		eval.Evaluate("naive", eval.NaiveResolver(), corpus, 0),
		eval.Evaluate("dictionary", eval.DictionaryResolver(), corpus, 0),
		eval.Evaluate("dictionary", eval.DictionaryResolver(), corpus, 0.5),
		eval.Evaluate("dictionary", eval.DictionaryResolver(), corpus, 0.9),
	}

	fmt.Printf("CPE resolution evaluation (n=%d)\n\n", len(corpus))
	fmt.Print(eval.Report(results))

	if !*showFailures {
		return
	}

	for _, m := range results {
		failures := eval.Failures(m)
		if failures == "" {
			continue
		}
		fmt.Fprintf(os.Stdout, "\n%s @ %.2f failures:\n%s", m.Name, m.Threshold, failures)
	}
}

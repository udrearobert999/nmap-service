package http

import (
	"encoding/json"
	"net/http"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type status struct {
	Status string `json:"status"`
}

func NewHealthHandler(checker domain.HealthChecker) http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("/healthz", func(w http.ResponseWriter, r *http.Request) {
		if err := checker.Ping(r.Context()); err != nil {
			w.WriteHeader(http.StatusServiceUnavailable)
			_ = json.NewEncoder(w).Encode(status{Status: "unavailable"})
			return
		}

		w.WriteHeader(http.StatusOK)
		_ = json.NewEncoder(w).Encode(status{Status: "ok"})
	})

	return mux
}

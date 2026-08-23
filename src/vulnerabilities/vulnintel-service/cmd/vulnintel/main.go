package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	"github.com/udrearobert999/vulnintel-service/internal/application"
	"github.com/udrearobert999/vulnintel-service/internal/config"
	"github.com/udrearobert999/vulnintel-service/internal/cpe"
	"github.com/udrearobert999/vulnintel-service/internal/cve"
	deliveryhttp "github.com/udrearobert999/vulnintel-service/internal/delivery/http"
	deliverykafka "github.com/udrearobert999/vulnintel-service/internal/delivery/kafka"
	"github.com/udrearobert999/vulnintel-service/internal/kev"
	"github.com/udrearobert999/vulnintel-service/internal/nvd"
	"github.com/udrearobert999/vulnintel-service/internal/repository/postgres"
	"github.com/udrearobert999/vulnintel-service/internal/scoring"
)

const (
	assessmentServiceName = "assessment"
	heartbeatInterval     = 10 * time.Second
	kevRefreshInterval    = 6 * time.Hour
	cveCacheTTL           = 7 * 24 * time.Hour
)

func main() {
	if err := run(); err != nil {
		slog.Error("vulnintel-service exited with error", "error", err)
		os.Exit(1)
	}
}

func run() error {
	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	cfg, err := config.Load()
	if err != nil {
		return err
	}

	pool, err := postgres.NewPool(ctx, cfg.PostgresDSN)
	if err != nil {
		return err
	}
	defer pool.Close()

	repo := postgres.NewRepository(pool)

	cpeResolver := cpe.NewResolver()
	nvdClient := nvd.NewClient()
	cveLookup := cve.NewCachedLookup(repo, nvdClient, cveCacheTTL)
	kevCatalog := kev.NewCatalog()
	scorer := scoring.NewScorer()

	service := application.NewScanRiskAssessmentService(cpeResolver, cveLookup, kevCatalog, scorer, repo, repo)

	brokers := strings.Split(cfg.KafkaBootstrapServers, ",")

	consumer := deliverykafka.NewConsumer(brokers, cfg.ScanRiskAssessmentRequestsTopic, cfg.KafkaConsumerGroupID)
	defer consumer.Close()

	httpServer := &http.Server{
		Addr:    ":" + cfg.HTTPPort,
		Handler: deliveryhttp.NewHealthHandler(repo),
	}

	errCh := make(chan error, 2)

	go func() {
		slog.Info("starting health server", "port", cfg.HTTPPort)
		if err := httpServer.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			errCh <- err
		}
	}()

	go func() {
		slog.Info("starting kafka consumer",
			"topic", cfg.ScanRiskAssessmentRequestsTopic, "group", cfg.KafkaConsumerGroupID)
		errCh <- consumer.Run(ctx, service.Assess)
	}()

	go runHeartbeat(ctx, repo)
	go kevCatalog.RunRefresh(ctx, kevRefreshInterval)

	select {
	case <-ctx.Done():
		slog.Info("shutting down")
	case err := <-errCh:
		if err != nil {
			return err
		}
	}

	return httpServer.Shutdown(context.Background())
}

func runHeartbeat(ctx context.Context, writer interface {
	UpsertHeartbeat(ctx context.Context, serviceName string) error
}) {
	ticker := time.NewTicker(heartbeatInterval)
	defer ticker.Stop()

	for {
		if err := writer.UpsertHeartbeat(ctx, assessmentServiceName); err != nil {
			slog.Error("failed to write assessment heartbeat", "error", err)
		}

		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
		}
	}
}

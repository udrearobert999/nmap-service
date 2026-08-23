package config

import (
	"fmt"
	"os"
)

type Config struct {
	HTTPPort string

	PostgresDSN string

	KafkaBootstrapServers           string
	KafkaConsumerGroupID            string
	ScanRiskAssessmentRequestsTopic string
}

func Load() (Config, error) {
	cfg := Config{
		HTTPPort:                        getEnv("HTTP_PORT", "8090"),
		PostgresDSN:                     os.Getenv("POSTGRES_DSN"),
		KafkaBootstrapServers:           getEnv("KAFKA_BOOTSTRAP_SERVERS", "localhost:9094"),
		KafkaConsumerGroupID:            getEnv("KAFKA_CONSUMER_GROUP_ID", "vulnintel-service"),
		ScanRiskAssessmentRequestsTopic: getEnv("KAFKA_SCAN_RISK_ASSESSMENT_REQUESTS_TOPIC", "scan-risk-assessment-requests-topic"),
	}

	if cfg.PostgresDSN == "" {
		return Config{}, fmt.Errorf("POSTGRES_DSN is required")
	}

	return cfg, nil
}

func getEnv(key, fallback string) string {
	if value, ok := os.LookupEnv(key); ok && value != "" {
		return value
	}
	return fallback
}

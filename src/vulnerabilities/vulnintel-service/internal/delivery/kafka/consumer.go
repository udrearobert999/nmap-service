package kafka

import (
	"context"
	"encoding/json"
	"fmt"
	"log/slog"

	kafkago "github.com/segmentio/kafka-go"

	"github.com/udrearobert999/vulnintel-service/internal/domain"
)

type Handler func(ctx context.Context, event domain.ScanRiskAssessmentRequested) error

type Consumer struct {
	reader *kafkago.Reader
}

func NewConsumer(brokers []string, topic, groupID string) *Consumer {
	return &Consumer{
		reader: kafkago.NewReader(kafkago.ReaderConfig{
			Brokers: brokers,
			Topic:   topic,
			GroupID: groupID,
		}),
	}
}

func (c *Consumer) Close() error {
	return c.reader.Close()
}

func (c *Consumer) Run(ctx context.Context, handle Handler) error {
	for {
		msg, err := c.reader.FetchMessage(ctx)
		if err != nil {
			if ctx.Err() != nil {
				return nil
			}
			return fmt.Errorf("fetching message: %w", err)
		}

		var event domain.ScanRiskAssessmentRequested
		if err := json.Unmarshal(msg.Value, &event); err != nil {
			slog.Error("failed to unmarshal scan-risk-assessment-requested event", "error", err)
		} else if err := handle(ctx, event); err != nil {
			slog.Error("failed to handle scan-risk-assessment-requested event",
				"scanRiskAssessmentId", event.ScanRiskAssessmentID, "error", err)
		}

		if err := c.reader.CommitMessages(ctx, msg); err != nil {
			slog.Error("failed to commit message", "error", err)
		}
	}
}

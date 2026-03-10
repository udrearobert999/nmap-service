curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: f1b49e3d-7d2a-4a8b-9e45-12c8b7f3a9d1" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: 9a8c2d1b-4f7e-4b6a-8c3d-2e1f4a5b6c7d" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: c3d4e5f6-a1b2-4c3d-8e7f-9a0b1c2d3e4f" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: e7f8a9b0-c1d2-4e3f-a5b6-c7d8e9f0a1b2" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: b1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: d5e6f7a8-b9c0-4d1e-8f2a-3b4c5d6e7f8a" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: f9a0b1c2-d3e4-4f5a-6b7c-8d9e0f1a2b3c" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: a2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: c4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f" \
  -d '{"target": "google.com"}'

curl -X POST http://localhost:8080/api/scans \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: e6f7a8b9-c0d1-4e2f-3a4b-5c6d7e8f9a0b" \
  -d '{"target": "google.com"}'

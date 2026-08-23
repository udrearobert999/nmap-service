# Vantage — Project Status (Multi-Tenant Network Security Platform)

Dissertation platform: security teams scan infrastructure and assess the risk of
what's found. Scanning and vulnerability risk-assessment are decoupled as
event-driven microservices (.NET + Go over Kafka), multi-tenant per team.

## Architecture

- **REST API** (`src/scans/Vantage.WebAPI`, .NET) — the "brain". Owns
  scans + risk-assessment lifecycle, publishes work via the transactional
  outbox, enforces auth + tenant scoping.
- **Scan worker** (`src/scans/Vantage.Worker`, .NET) — consumes
  `scan-requests-topic`, runs `nmap -sV`, writes results to the shared DB.
- **vulnintel-service** (`src/vulnerabilities/vulnintel-service`, Go, clean
  architecture) — consumes `scan-risk-assessment-requests-topic`, does
  CPE→CVE matching + scoring, writes status/findings to the shared DB.
- **Portal** (`src/portal`, React + Vite + shadcn/ui) — dark, monospace
  dashboard UI. Clerk for auth/orgs.
- **Shared Postgres** `VantageDb` (one EF Core `AppDbContext`), **Kafka**,
  all orchestrated by `compose.yaml`.

Patterns: Clean Architecture, Repository, Unit of Work, Transactional Outbox
(dispatch-by-type), Atomic Claim (`FOR UPDATE SKIP LOCKED`), idempotency via
`X-Idempotency-Key` + unique `RequestId`.

## Done

### Scanning
- `POST /api/scans`, `GET /api/scans` (paged/filtered/cached), `GET /api/scans/{id}`,
  `GET /api/scans/diff/{target}`.
- `nmap -sV` version detection → `ScanResult.Product`/`Version` parsed and stored.
- 202 + `Location` header + created DTO on create.

### Risk assessments (`ScanRiskAssessment`)
- 1-for-1 clone of the scanning pattern (outbox → Kafka → Go worker → shared DB).
- `POST /api/scans/{scanId}/risk-assessments` (guardrail: scan exists + Completed +
  has results), `GET /api/risk-assessments` (paged/filtered), `GET /api/risk-assessments/{id}`,
  `GET /api/scans/{scanId}/risk-assessments`, `GET /api/scans/{scanId}/risk-assessment` (latest).
- **CVE/CPE engine** (Go): CPE resolver (curated vendor:product dict + version
  normalization + confidence), live **NVD** lookup, **CISA KEV** catalog,
  local **CVE cache** (7-day TTL), weighted scoring (CVSS × KEV, heuristic tier
  for CVE-less risky services). Findings persisted + exposed on the API.

### Multi-tenancy + auth (Clerk Organizations)
- `User` + `Team` (= Clerk org) + `TeamMembership` (role), JIT-mirrored from the
  Clerk JWT on each request (`IdentitySyncService`).
- Active-org scoping via EF Core global query filter; **no active org → 403**.
- `CreatedByUserId` stamped on scans/assessments; `createdByEmail` exposed.
- Clerk JWT replaces Auth0. Dev-only header bypass
  (`X-Dev-Subject`/`X-Dev-Org`/`X-Dev-Role`/`X-Dev-Email`) for offline testing.

### Connectivity
- `GET /api/health` → db + per-service (`scanning`/`assessment`) status from a
  `ServiceHeartbeats` table (each worker upserts every ~10s).

### Portal (UI)
- Clerk sign-in, `<OrganizationSwitcher>`/`<UserButton>`, active-org gate.
- Dark neutral + monospace theme, shadcn sidebar shell.
- Dashboard (`/`): stat cards + scan-activity area chart (recharts).
- Scans list (`/scans`) + detail with results, risk-assessment panel (findings
  table: CVSS/KEV/confidence badges, matched CVEs), connectivity dots.
- Clerk widgets dark-themed via `@clerk/themes`.

### Verified
- .NET build + 22 unit tests green; Go build/vet/gofmt + cpe/scoring tests green.
- Full stack e2e in Docker: scan → risk assessment → real CVE findings (e.g.
  PostgreSQL → 20 CVEs, CVSS 9.8); tenant isolation; connectivity dots.
- Portal `npm run build` green; dashboard visually verified against the design.

## Still to do

- **Portal runtime verification** — needs a real Clerk `pk_test_` key (dev
  instance, Organizations enabled) in `src/portal/.env.local`; only build +
  mock-data screenshot done so far.
- **Real Clerk JWT validation on the backend** — today Development uses the dev
  header bypass; wire `Clerk__Authority` for non-Development and test with real
  tokens.
- **Clerk webhooks** — JIT sync only reflects the active caller; full member
  roster + removals need `user.*`/`organization*.*` webhooks (needs a public URL).
- **Dashboard aggregate endpoint** — stats are computed client-side over a
  25-item page (avg risk is "recent"); add `/dashboard/summary` for exact totals.
- **CPE matching evaluation** — precision/recall vs. a naive baseline against
  known-vulnerable targets (Metasploitable2/DVWA) — the dissertation's headline
  empirical contribution.
- **NVD coverage** — CVEs are fetched on demand + cached (not a full mirror);
  first assessment of a new service makes a network call. Rate-limit/backoff and
  a fuller sync are future work.
- **Scan-diff / dashboards / risk-trend** UI pages (backend diff exists; no UI).
- **Kubernetes** — deferred; everything runs on Docker Compose.
- **Distro-patched false positives** — naive version matching flags e.g.
  Ubuntu-backported Apache; treat as a confidence-tier limitation.
- **Known nit**: on a cold `docker compose up`, vulnintel logs one heartbeat
  error before migrations apply, then self-heals.

## Run

```bash
docker compose up            # everything: portal, API, workers, Kafka, DB
```
- Portal (UI): http://localhost:5173  (set `VITE_CLERK_PUBLISHABLE_KEY` in `.env`)
- API + Swagger: http://localhost:8080/swagger
- Kafka UI: http://localhost:8081

Dev auth without Clerk: send `X-Dev-Subject` + `X-Dev-Org` + `X-Dev-Role`
(+ `X-Dev-Email`) headers.

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

### Auth modes
`Auth__Mode` selects how requests are authenticated, independently of
`ASPNETCORE_ENVIRONMENT`:

- `DevHeaders` — trusts `X-Dev-*` headers. Refuses to start outside Development.
- `Clerk` — validates real Clerk JWTs against `Clerk__Authority` (optionally
  `Clerk__Audience`). Startup fails fast if the authority is missing.

Blank falls back to `DevHeaders` in Development and `Clerk` elsewhere, so
`AUTH_MODE=Clerk` + `CLERK_AUTHORITY=https://<slug>.clerk.accounts.dev` lets you
exercise real token validation locally with Swagger still available.

Clerk's v2 session token nests the active organisation under an `o` claim
(`{"id": "...", "rol": "admin"}`) rather than flat `org_id`/`org_role`; identity
resolution reads both shapes and strips an `org:` role prefix.

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

### CPE matching evaluation
Reproduce with `go run ./cmd/cpe-eval [-failures]` (or
`go test ./internal/cpe/eval/ -v`) in `src/vulnerabilities/vulnintel-service`.

A hand-labelled corpus of 34 real `nmap -sV` banners (Metasploitable2, DVWA,
common services, plus a held-out set of products deliberately absent from the
dictionary) is scored against a naive baseline that lowercases the product into
both vendor and product and takes the version verbatim. A sample counts as a
true positive only when the emitted CPE matches the label exactly and clears the
confidence threshold; banners with no catalogued product must be abstained on.

| resolver | threshold | precision | recall | F1 |
|---|---|---|---|---|
| naive | 0.00 | 0.333 | 1.000 | 0.500 |
| dictionary | 0.00 | 0.818 | 1.000 | 0.900 |
| dictionary | 0.50 | 1.000 | 0.742 | 0.852 |
| dictionary | 0.90 | 1.000 | 0.677 | 0.808 |

Findings:
- Dictionary + version normalisation lifts F1 from 0.500 to 0.900 at the same
  recall; the naive baseline's precision collapses because distro-patched
  versions (`4.7p1 Debian 8ubuntu1`) and compound product strings never match.
- The confidence tier is what buys precision: rejecting below 0.5 removes every
  false positive (1.000 precision) at the cost of recall, so the score is a
  usable abstention signal rather than decoration.
- The evaluation found two real defects, now fixed: MySQL was mapped to
  `mysql:mysql` when NVD publishes `oracle:mysql` (so MySQL CVE lookups silently
  returned nothing), and Tomcat's actual banner
  (`Apache Tomcat/Coyote JSP engine`) fell back to `apache:apache`.
- Open calibration gap: 4 of the 8 rejections at 0.5 (OpenLDAP, HAProxy,
  Memcached, Dovecot) emitted the *correct* CPE but scored 0.40. The fallback is
  right more often than its confidence admits whenever vendor equals product;
  raising that case would recover recall without costing precision.

Caveat: the corpus is hand-labelled and small, and the dictionary was corrected
in response to it, so in-dictionary figures are optimistic. The held-out block
is the generalisation signal.

### Verified
- .NET build + 22 unit tests green; Go build/vet/gofmt + cpe/scoring/eval tests
  green.
- Full stack e2e in Docker: scan → risk assessment → real CVE findings (e.g.
  PostgreSQL → 20 CVEs, CVSS 9.8); tenant isolation; connectivity dots.
- Portal `npm run build` green; dashboard visually verified against the design.

## Still to do

- **Clerk session-token custom claims** — Clerk's v2 session token carries no
  `email`/`name`, so under `Auth__Mode=Clerk` the "created by" column is blank.
  Add them in the Clerk dashboard (Sessions → customise session token) as
  `{"email": "{{user.primary_email_address}}", "name": "{{user.full_name}}"}`.
- **Clerk webhooks** — JIT sync only reflects the active caller; full member
  roster + removals need `user.*`/`organization*.*` webhooks (needs a public URL).
- **CPE confidence calibration** — the evaluation shows the fallback emits the
  correct CPE at 0.40 confidence whenever vendor equals product; raising that
  case (and widening the corpus) is the obvious next experiment.
- **NVD coverage** — CVEs are fetched on demand + cached (not a full mirror);
  first assessment of a new service makes a network call. The client now
  rate-limits and retries (see below), but a fuller offline sync is future work.
- **Risk-trend UI** — scan diff now has a page; per-target risk-score history
  over time does not.
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

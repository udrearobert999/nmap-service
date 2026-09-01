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
  `GET /api/scans/{scanId}/risk-assessments`, `GET /api/scans/{scanId}/risk-assessment` (latest),
  `GET /api/risk-assessments/trend/{target}` (every Completed assessment for a
  target, ordered by `CompletedAt` — the per-target risk-score-over-time series
  the portal's Risk trend page charts).
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

### Clerk webhooks
`POST /api/webhooks/clerk` (anonymous) keeps the local roster in step with Clerk
for users who never call the API, which JIT sync cannot do.

Every request is verified as a Svix signature: HMAC-SHA256 over
`{svix-id}.{svix-timestamp}.{raw body}` compared in constant time, with a
five-minute timestamp window so captured deliveries cannot be replayed. An
unsigned, tampered or stale request is rejected with 401 and never reaches a
handler. With no signing secret configured the endpoint answers 503 rather than
accepting unverified events.

Handled events: `user.created/updated/deleted`,
`organization.created/updated/deleted`,
`organizationMembership.created/updated/deleted`. Unknown types are acknowledged
with 200 and ignored, so Svix does not retry events the system does not model.

Two deliberate choices:
- `user.deleted` revokes the user's memberships but keeps the `User` row.
  `Scan.CreatedByUserId` is `DeleteBehavior.Restrict`, and deleting authorship
  history to satisfy a membership change would be wrong for an audit trail.
- `organization.deleted` deletes the `Team`, which cascades that tenant's scans
  and assessments. The tenant no longer exists, and the rows would otherwise be
  unreachable behind the team query filter.

**Live delivery verified end-to-end** via the Clerk endpoint at
`https://webhooks.clerk.com/in/c_cIv23iBHsu/` (a pinned relay from
`clerk webhooks listen`, forwarded to the local API) with a real signing secret
in `CLERK_WEBHOOK_SIGNING_SECRET`. Created, updated and deleted a real
organization through the Clerk API and confirmed each event was delivered,
signature-verified, and applied: the `Team` row appeared on `organization.created`,
its name changed on `organization.updated`, and it (and its cascaded rows) was
removed on `organization.deleted`. Real secrets for local runs go in
`.env.development` (git-ignored; `docker compose --env-file .env.development up`),
copied from the tracked `.env` template.

`session.claims` was also set on the Clerk instance
(`{"email": "{{user.primary_email_address}}", "name": "{{user.full_name}}"}`)
via `clerk config patch`, and verified live: a real JWT now carries an `email`
claim.

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
- Cold-start fix: `vulnintel-service` writes its first heartbeat immediately on
  boot (`cmd/vulnintel/main.go` — `runHeartbeat` upserts before waiting on the
  ticker), but `ServiceHeartbeats` only exists once `api`'s EF Core migrations
  have run (`app.ApplyMigrations()` before `app.Run()` in `Program.cs`).
  `compose.yaml` used to only gate `worker`/`vulnintel-service` on
  `postgres: service_healthy` — accepting connections, not "schema exists" —
  so a fresh volume could race the first heartbeat write against migrations.
  Fixed by adding a real healthcheck to `api` (`curl -f
  http://localhost:8080/api/health`, needs `curl` in the API image) and making
  `worker`/`vulnintel-service` depend on `api: service_healthy`; since Kestrel
  doesn't start listening until migrations finish, a passing healthcheck is a
  correct migrations-done signal.

### Dev seed data
`scripts/seed-dev-data.sql` seeds 3 synthetic targets (`10.20.30.40-42`)
telling declining/rising/stable risk-score stories, for exercising the diff
and risk-trend UI locally. Idempotent (deletes/recreates only those targets
under the chosen team) and parameterized — defaults to `org_e2e`/`dev|e2e-user`
(API-only, won't show in the browser — see Portal note above), or pass
`-v team_id=<uuid> -v user_id=<uuid>` for a real signed-in team's UUIDs (not
its Clerk org id):
```bash
docker exec -i vantage_db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
  -v team_id=<team-uuid> -v user_id=<user-uuid> -f - < scripts/seed-dev-data.sql
```

### Portal (UI)
- Clerk sign-in, `<OrganizationSwitcher>`/`<UserButton>`, active-org gate. The
  portal always requires a real Clerk sign-in in the browser — `Auth__Mode`'s
  dev-header bypass is API-only (curl/integration tests), the frontend has no
  equivalent bypass, so a team seeded under a synthetic dev id (e.g. `org_e2e`)
  never appears in the browser regardless of backend auth mode.
- Dark neutral + monospace theme, shadcn sidebar shell.
- Dashboard (`/`): stat cards + scan-activity area chart (recharts).
- Scans list (`/scans`) + detail with results, risk-assessment panel (findings
  table: CVSS/KEV/confidence badges, matched CVEs), connectivity dots.
- Scan diff (`/diff`): compare two completed scans of the same target.
- Risk trend (`/risk-trend`): per-target overall-risk-score line chart across
  every completed assessment, backed by the trend endpoint above.
- Clerk widgets dark-themed via `@clerk/themes`.

Two portal instances can be running locally at once and they are not the same
code: `docker compose`'s `portal` service (`:5173`) runs `npm run dev` *inside
the container*, rebuilt from `COPY . .` at image-build time, so host edits
need `docker compose up -d --build portal` to appear there. A separate local
`vite` dev server on `:5174` (`VITE_API_PROXY_TARGET` defaults to
`localhost:8080`) proxies to the same containerized API/DB and hot-reloads
host edits immediately — useful for iterating without rebuilding the image.

### CPE matching evaluation
Reproduce with `go run ./cmd/cpe-eval [-failures]` (or
`go test ./internal/cpe/eval/ -v`) in `src/vulnerabilities/vulnintel-service`.

A hand-labelled corpus of 71 real `nmap -sV` banners (Metasploitable2, DVWA,
common services, an edge-case block covering every dictionary entry and messy
version strings that weren't previously exercised, plus a held-out set of
products deliberately absent from the dictionary — 24 of them, each verified
against the live NVD CPE dictionary API before being added) is scored against
a naive baseline that lowercases the product into both vendor and product and
takes the version verbatim. A sample counts as a true positive only when the
emitted CPE matches the label exactly and clears the confidence threshold;
banners with no catalogued product must be abstained on.

| resolver | threshold | precision | recall | F1 |
|---|---|---|---|---|
| naive | 0.00 | 0.271 | 1.000 | 0.427 |
| dictionary | 0.00 | 0.757 | 1.000 | 0.862 |
| dictionary | 0.50 | 1.000 | 0.647 | 0.786 |
| dictionary | 0.90 | 1.000 | 0.603 | 0.752 |

Findings:
- Dictionary + version normalisation lifts F1 well above the naive baseline at
  the same recall; the naive baseline's precision collapses because
  distro-patched versions (`4.7p1 Debian 8ubuntu1`) and compound product
  strings never match.
- The confidence tier is what buys precision: rejecting below 0.5 removes every
  false positive (1.000 precision) at the cost of recall, so the score is a
  usable abstention signal rather than decoration.
- The evaluation found two real defects, now fixed: MySQL was mapped to
  `mysql:mysql` when NVD publishes `oracle:mysql` (so MySQL CVE lookups silently
  returned nothing), and Tomcat's actual banner
  (`Apache Tomcat/Coyote JSP engine`) fell back to `apache:apache`.
- **Calibration gap, tested and closed**: the original 8-sample held-out set
  suggested the fallback's vendor==product guess was "usually right" (4/4
  correct at 0.40 confidence) and that raising its confidence would recover
  recall for free. Widening the held-out set to 24 NVD-verified samples
  disproved that harder each time: at 15 samples the guess was right 7/15
  (46.7%); at 24 it's right 9/24 (37.5%) — Grafana, Zabbix, Fluentd, GitLab,
  Jenkins, OpenLDAP, HAProxy, Memcached, Dovecot right; Squid, ISC DHCPD,
  RabbitMQ, Werkzeug, Consul, CouchDB, ZooKeeper, Traefik, Varnish, InfluxDB,
  Nomad, Vault, Cassandra, Kafka, ActiveMQ wrong. The wrong cases cluster
  around real orgs/foundations behind a single-word project name (Apache
  Software Foundation alone accounts for five: Cassandra, Kafka, ActiveMQ,
  ZooKeeper, CouchDB; HashiCorp for three: Consul, Nomad, Vault) — a
  structural pattern the resolver has no way to detect from the banner text
  alone. Raising confidence past the 0.5 acceptance threshold would trade
  precision for recall, not gain recall for free, and the wider sample makes
  that tradeoff look worse, not better. Fallback confidence (with a version) is
  0.45 — matching the measured rate, not the small-sample rate — instead of
  the original 0.40, but deliberately still below 0.5.

Caveat: the corpus is hand-labelled, and the dictionary was corrected in
response to it, so in-dictionary figures are optimistic. The held-out block is
the generalisation signal, and is now large enough (n=24) that the calibration
finding above is unlikely to be a small-sample artifact the way the original
n=8 conclusion was.

### Verified
- .NET build + 22 unit tests green; Go build/vet/gofmt + cpe/scoring/eval tests
  green.
- Full stack e2e in Docker: scan → risk assessment → real CVE findings (e.g.
  PostgreSQL → 20 CVEs, CVSS 9.8); tenant isolation; connectivity dots.
- Portal `npm run build` green; dashboard visually verified against the design.
- Risk trend endpoint + page verified against seeded data in a real
  browser session (signed-in Clerk org): correct scores, correct tenant
  isolation (a different team sees an empty trend, not another team's data),
  400 on an invalid target.
- Compose healthcheck fix verified structurally (dependency ordering now
  correctly gates on `api` reaching healthy) via a warm restart; not
  reproduced against a wiped Postgres volume, since that would have destroyed
  the real scan/Clerk data from earlier webhook verification in this repo.

## Still to do

- **NVD coverage** — CVEs are fetched on demand + cached (not a full mirror);
  first assessment of a new service makes a network call. The client now
  rate-limits and retries (see below), but a fuller offline sync is future work.
  A live CPE-dictionary lookup (rather than the static confidence heuristic)
  is also the only way to move the CPE fallback's confidence past its current
  37.5%-measured ceiling — see the calibration finding above.
- **Kubernetes** — deferred; everything runs on Docker Compose.
- **Distro-patched false positives** — naive version matching flags e.g.
  Ubuntu-backported Apache; treat as a confidence-tier limitation.

## Run

```bash
docker compose up            # everything: portal, API, workers, Kafka, DB
```
- Portal (UI): http://localhost:5173  (set `VITE_CLERK_PUBLISHABLE_KEY` in `.env`)
- API + Swagger: http://localhost:8080/swagger
- Kafka UI: http://localhost:8081

Dev auth without Clerk: send `X-Dev-Subject` + `X-Dev-Org` + `X-Dev-Role`
(+ `X-Dev-Email`) headers.

# Vantage — Architecture & Design (Multi-Tenant Network Security Platform)

A deep reference for **every technology, architectural pattern, system-design
pattern, and code-level pattern** used in this project, and **why** each was
chosen. Written to be the canonical explanation of *how* the system is built and
*why it is built that way*.

---

## Table of contents

1. [Problem domain & goals](#1-problem-domain--goals)
2. [High-level architecture](#2-high-level-architecture)
3. [Technology stack (and why)](#3-technology-stack-and-why)
4. [Architectural patterns](#4-architectural-patterns)
5. [System-design / integration patterns](#5-system-design--integration-patterns)
6. [Code-level / tactical patterns](#6-code-level--tactical-patterns)
7. [Data model & persistence design](#7-data-model--persistence-design)
8. [Security & multi-tenancy design](#8-security--multi-tenancy-design)
9. [The vulnerability-intelligence domain](#9-the-vulnerability-intelligence-domain)
10. [Testing strategy](#10-testing-strategy)
11. [Key trade-offs & limitations](#11-key-trade-offs--limitations)

---

## 1. Problem domain & goals

Security teams need to (a) **scan** their infrastructure to discover exposed
services, and (b) **assess the risk** of those services against known
vulnerabilities. These are *different concerns with different characteristics*:

- Scanning is I/O-bound, slow, and network-facing (running `nmap`).
- Risk assessment is data/CPU-bound and depends on external intelligence feeds
  (NVD, CISA KEV) that evolve independently.

The **design goal** is to decouple these so they can evolve, scale, fail, and be
reasoned about independently — while keeping every team's data **strictly
isolated** (multi-tenant). This maps directly onto an **event-driven
microservices** architecture with a **polyglot** implementation, which is the
dissertation's core thesis: *decoupling scanning from vulnerability correlation
as independent, event-driven services.*

---

## 2. High-level architecture

```
                          ┌──────────────────────────────────────┐
   Browser (Clerk auth)   │            Portal (React)            │  :5173
        │                 │   dark shadcn/ui dashboard + SPA     │
        │  /api (proxy)   └──────────────────────────────────────┘
        ▼
┌─────────────────────┐   scan-requests-topic    ┌──────────────────────┐
│   REST API (.NET)   │ ───────────────────────▶ │  Scan worker (.NET)  │
│   "the brain"       │                          │   runs nmap -sV      │
│                     │ ◀───── shared DB ──────── │                      │
│  - scans            │                          └──────────────────────┘
│  - risk assessments │
│  - auth + tenancy   │   scan-risk-assessment-  ┌──────────────────────┐
│  - outbox publisher │   requests-topic         │ vulnintel-service    │
│                     │ ───────────────────────▶ │  (Go)                │
└─────────────────────┘                          │  CPE→CVE + scoring   │
        │      ▲                                  │  NVD + CISA KEV      │
        │      │ shared DB (VantageDb)      └──────────────────────┘
        ▼      │                                              │
┌──────────────────────────────────────────────────────────────────────┐
│           PostgreSQL  (single EF Core AppDbContext, shared)            │
│  Scans · ScanResults · ScanRiskAssessments · Findings · CveCache ·     │
│  Teams · Users · TeamMemberships · OutboxMessages · ServiceHeartbeats  │
└──────────────────────────────────────────────────────────────────────┘
                     Kafka (KRaft, single broker) + Kafka UI
              Everything orchestrated by a single docker-compose
```

**Data-flow narrative:** the API accepts a request, writes the business row and
an outbox row in **one transaction**, returns `202 Accepted`. A background job
publishes the outbox row to Kafka. The relevant worker consumes it, does the
slow work, and writes the result back to the shared DB. Clients **poll** the API
for status. No synchronous coupling exists between the API and the workers.

---

## 3. Technology stack (and why)

### Backend runtimes

| Tech | Where | Why this and not alternatives |
|---|---|---|
| **.NET 10 / C#** | API, scan worker | Strong typing, mature EF Core, first-class DI/hosting, excellent async. The "brain" and the nmap worker share a domain, so one language for them reduces friction. |
| **Go** | vulnintel-service | The dissertation calls for a **polyglot** architecture to prove the services are truly decoupled (only Kafka + a shared schema contract couples them). Go is idiomatic for a lean, concurrent network/data service with tiny memory footprint and fast cold-start. |

**Why polyglot at all?** It forces the boundary to be honest: the only thing the
Go service shares with .NET is the Kafka message contract (JSON) and the DB
table schema — there is no shared code, no shared runtime. If the boundary were
sloppy, a polyglot split would break immediately.

### Messaging

| Tech | Why |
|---|---|
| **Apache Kafka (KRaft mode, single broker)** | Durable, replayable log; consumer groups give competing-consumers + rebalancing for free. KRaft (no ZooKeeper) keeps the local footprint small. Single broker is a deliberate scope choice (see limitations). |
| **MassTransit (.NET Kafka rider)** | Producer/consumer abstraction for the **scan** flow (.NET↔.NET). Handles serialization, endpoint config, concurrency limits, checkpointing. |
| **Raw `Confluent.Kafka` producer (.NET)** | For the **risk-assessment** topic, we bypass MassTransit. *Reason:* MassTransit wraps payloads in its own envelope, which the Go consumer (plain JSON) can't read. This was found and fixed by reading MassTransit's source, not docs. |
| **`segmentio/kafka-go` (Go)** | Lightweight, dependency-free Kafka client for the Go consumer + heartbeat. No MassTransit equivalent needed. |

### Persistence

| Tech | Why |
|---|---|
| **PostgreSQL** | Rock-solid relational store; supports the exact primitives the design leans on: `FOR UPDATE SKIP LOCKED` (atomic claim), unique constraints (idempotency), `ON CONFLICT` upserts (heartbeats/cache), and JSON-friendly `now()`/uuid. |
| **EF Core (Npgsql)** | The single source of truth for the schema (code-first migrations). Global query filters give **structural multi-tenancy**. One `AppDbContext` so tables can be joined. |
| **`pgx/v5` (Go)** | High-performance native Postgres driver for Go; the Go service writes status/findings and reads the CVE cache with plain parameterized SQL (no ORM needed for a handful of statements). |

### Supporting libraries

| Tech | Why |
|---|---|
| **Quartz.NET** | Schedules the outbox-publisher job every 2s. A proven scheduler beats hand-rolling a timer loop (misfire handling, DI integration, hosted-service lifecycle). |
| **FluentValidation** | Declarative, composable request validation; validators are auto-discovered by assembly scan and can inject the UoW for DB-backed rules (the risk-assessment guardrail). |
| **Newtonsoft.Json** | Used for outbox payload (de)serialization with explicit camelCase + reference-loop handling to match the Go consumer's expectations exactly. |
| **Serilog** | Structured logging with console + rolling file sinks, configured from `appsettings`. |
| **Scrutor** | Assembly-scanning DI registration (convention over configuration: classes ending in `Service`, all `IValidator<>`, all `IRepository<>`). |

### External intelligence

| Source | Why |
|---|---|
| **`nmap` (with `-sV`)** | The de-facto network scanner. `-sV` adds service/version detection, which is *required* to produce CVE-matchable signal (product + version). |
| **NVD 2.0 REST API** | Authoritative CVE + CVSS data, queryable by CPE. Fetched on demand and cached locally. |
| **CISA KEV catalog** | The list of *known-exploited* vulnerabilities; drives the exploit-maturity weighting in scoring. |

### Auth & frontend

| Tech | Why |
|---|---|
| **Clerk (Organizations)** | Auth + orgs + roles + invitations + management UI **out of the box**. Organizations map 1:1 onto our "team" tenant model, so we get member management and an org switcher without building any of it. |
| **React + Vite + TypeScript** | Fast SPA dev loop; TS for safety against the API contract. |
| **Tailwind CSS + shadcn/ui** | Utility-first styling + accessible, copy-in component primitives (sidebar, chart, table, dialog…). The dashboard is the shadcn "dashboard-01" block themed dark + monospace. |
| **TanStack Query** | Server-state caching + **polling** (the async-request/reply UX depends on it) + cache invalidation on org switch. |
| **recharts** | The charting engine behind shadcn's `chart`; used for the scan-activity area chart. |
| **react-router, sonner** | Routing and toasts. |
| **`@clerk/themes`** | Dark-themes the Clerk widgets to match the app. |

### Orchestration

| Tech | Why |
|---|---|
| **Docker Compose** | One command (`docker compose up`) runs the whole system — DB, Kafka, API, both workers, the portal, and Kafka UI. Kubernetes was explicitly deferred as out of scope. |

---

## 4. Architectural patterns

### 4.1 Clean Architecture (both .NET and Go)

Concentric layers with the **dependency rule** pointing inward (outer layers
depend on inner abstractions, never the reverse):

**.NET** (`src/scans/`):
- `Domain` — entities, value objects, `Result<T>`, repository/UoW **interfaces**. No framework dependencies.
- `Application` — use-case services, validators, mappers. Depends only on Domain + Contracts.
- `Contracts` — DTOs / message contracts shared across boundaries.
- `Infrastructure` / `Infrastructure.Persistence` — EF Core, Kafka, Quartz — the concrete implementations of Domain interfaces.
- `WebAPI` / `Worker` — the presentation/host layer (controllers, middleware, background services).

**Go** (`vulnintel-service/internal/`):
- `domain` — entities + **ports** (interfaces): `StatusWriter`, `FindingsWriter`, `CPEResolver`, `CVELookup`, `KEVCatalog`, `Scorer`, `ScanRiskAssessmentService`.
- `application` — the service orchestrating the ports.
- `repository/postgres`, `delivery/kafka`, `delivery/http`, `cpe`, `cve`, `nvd`, `kev`, `scoring` — adapters implementing the ports.

**Why:** business logic stays testable and independent of nmap, Kafka, Postgres,
or HTTP. Swapping any of those touches only an adapter. It's also the structure
the dissertation is evaluated on.

### 4.2 Microservices + Event Choreography

There is **no orchestrator**. Each service reacts to events and emits work via
the log. The API doesn't call the workers; it drops an event and forgets. This
is **choreography**, not orchestration — chosen because the steps are simple and
independent, so a central coordinator would add coupling for no benefit.

### 4.3 Polyglot with a shared-database boundary

We deliberately chose a **shared database** (`VantageDb`, one
`AppDbContext`) rather than database-per-service. The Go worker writes the
`ScanRiskAssessment` status/findings into the same DB the .NET API reads — the
*exact analogue* of how the .NET scan worker writes scan results back.

- **Why shared DB:** simplicity and strong read-your-writes consistency for the
  poll-based UX; it makes the risk-assessment worker a 1-for-1 clone of the scan
  worker. The alternative (own DB + a result event back to .NET) is cleaner for
  independent scaling but adds a second topic + a consumer + eventual
  consistency. This was an explicit, documented trade-off.
- **The boundary is still real:** Go shares *no code* with .NET; it depends only
  on the Kafka JSON contract and the table shape.

### 4.4 Layered read/write separation (light CQRS)

Reads and writes take different paths: writes go through the domain
services + UoW + outbox; reads use dedicated repository query methods
(`GetPagedAsync`, `GetWithFindingsByIdAsync`) with `AsNoTracking` projections and
output caching. It is *not* full CQRS (no separate models/stores), but it borrows
the separation where it pays off.

---

## 5. System-design / integration patterns

### 5.1 Transactional Outbox (the dual-write problem)

**Problem:** writing a business row to the DB *and* publishing to Kafka are two
systems; if one succeeds and the other fails, state diverges.

**Solution:** in a single DB transaction we write both the business row (e.g.
`Scan`) **and** an `OutboxMessage`. A separate Quartz job later reads pending
outbox rows and publishes them to Kafka, marking them completed. If publishing
fails, the row stays pending and is retried — **at-least-once** delivery with no
dual-write inconsistency.

**Generalization:** the outbox was refactored from a single hard-coded message
type into a **dispatch-by-type** design — `OutboxMessage.Type` selects an
`IOutboxMessagePublisher` implementation (Strategy pattern). This is how one
outbox table + one job serves both `ScanRequestMessage` (via MassTransit) and
`ScanRiskAssessmentRequestedMessage` (via the raw producer).

### 5.2 Atomic Claim / Competing Consumers (work not double-processed)

Multiple workers (and multiple outbox-job runs) may race for the same row.

- **Outbox claim:** `UPDATE … WHERE Status='Pending' … FOR UPDATE SKIP LOCKED …
  RETURNING *` — a batch of rows is atomically claimed; concurrent claimers skip
  locked rows instead of blocking. Classic Postgres work-queue pattern.
- **Scan claim:** `ExecuteUpdate` flips `Pending → Running` and returns
  rows-affected; only the winner (`rowsAffected > 0`) proceeds.
- **Kafka consumer groups** give competing consumers across service replicas with
  automatic partition rebalancing.

### 5.3 Idempotency

`POST` creates carry an `X-Idempotency-Key`, stored as a **unique** `RequestId`.
The create path checks for an existing row first, and the save is wrapped in a
**concurrency-safe** try/catch: if two identical requests race and one hits the
unique-constraint violation, we catch it and return the already-created row
instead of erroring. Safe to retry; no duplicate scans/assessments.

### 5.4 Async Request-Reply (fire-and-forget + poll)

Creates return `202 Accepted` + a `Location` header + the created id. The client
**polls** `GET …/{id}` (via TanStack Query's `refetchInterval`, which stops once
the status is terminal). This keeps the API responsive while slow work runs in
the background, and needs no callback infrastructure.

### 5.5 Cache-Aside

- **CVE cache:** on assessment, the Go service checks the local
  `CveCacheEntries` table first (7-day TTL); on a miss it calls NVD and populates
  the cache. Repeat assessments of the same CPE skip the network entirely
  (verified: cache timestamp unchanged on the second run).
- **HTTP output cache:** the scans/assessments list endpoints are output-cached
  and **evicted by tag** on create.

### 5.6a Client-side rate limiting & retry (NVD)

The NVD client self-throttles to NVD's published budget — roughly one request
per 6.5s anonymously, or per 0.7s when `NVD_API_KEY` is set — using a
monotonic next-slot clock so concurrent assessments queue instead of bursting.
Failures are retried up to four times with exponential backoff plus jitter,
honouring a `Retry-After` header when the server sends one. Only throttling and
server-side faults (429, 403, 5xx, transport errors) are retried; a malformed
request (4xx) fails immediately rather than burning the budget. Every wait is
context-aware, so a cancelled assessment stops sleeping straight away.

### 5.6 Graceful degradation

If NVD is unreachable, the CVE lookup logs a warning and returns what it has
(possibly empty) rather than failing the whole assessment — the assessment still
completes with whatever signal is available. External-dependency failure is
contained, not propagated.

### 5.7 Heartbeat / Health-check

Each worker upserts a row into `ServiceHeartbeats` every ~10s
(`ON CONFLICT … DO UPDATE`). `GET /api/health` reports a service `up` if its
heartbeat is within 30s. This proves a worker is *both alive and DB-connected*,
and drives the portal's green/red connectivity dots. Killing the Go container
turns the "assessment" dot red within 30s and it recovers on restart — the
demonstrable proof of the async design's resilience.

### 5.8 Backpressure / concurrency control

The scan consumer sets `PrefetchCount`, `ConcurrentConsumerLimit`,
`ConcurrentMessageLimit`, and a checkpoint interval — bounding how much work is
pulled and processed at once so a burst of scans can't overwhelm the worker.

---

## 6. Code-level / tactical patterns

- **Repository** — `IRepository<TEntity,TKey>` / `IReadOnlyRepository<>` plus
  specialized repos (`IScanRepository`, `IScanRiskAssessmentRepository`) abstract
  persistence from the application layer.
- **Unit of Work** — `IUnitOfWork` owns the repositories + `SaveChangesAsync` +
  transaction control, so a business operation (row + outbox) commits atomically.
- **Ports & Adapters (Hexagonal)** — the Go service in particular: domain defines
  ports, adapters implement them, `main` wires them. Compile-time interface
  assertions (`var _ domain.X = (*impl)(nil)`) enforce the contracts.
- **Result pattern** — `Result` / `Result<T>` model success/validation/not-found
  as *values*, not exceptions. Controllers map results to HTTP status via
  `ApiControllerBase.HandleFailure`. Flow control without exception abuse.
- **Dependency Injection + convention registration** — Scrutor scans assemblies
  and registers by convention (name/interface), so adding a `…Service` or an
  `AbstractValidator<>` needs no manual wiring.
- **Options pattern** — `IOptions<KafkaOptions>` / `ClerkOptions` bind config
  sections; env vars override via the double-underscore convention in Compose.
- **DTO + Mapper** — `Contracts` DTOs never leak entities; static extension-method
  mappers (`ToDto`, `ToGetResponse`, `ToEntity`, `ToOutboxMessage`) convert.
- **Validation orchestrator** — a thin service resolves all `IValidator<T>` for a
  request and aggregates failures into the `Result` error model.
- **Factory** — `ErrorFactory` centralizes error construction.
- **Middleware pipeline** — global exception handler, and the
  `IdentityResolutionMiddleware` that resolves tenant/user per request.
- **Background services** — .NET `IHostedService`/`BackgroundService` (heartbeat,
  Quartz job) and Go goroutines (consumer loop, heartbeat, KEV refresh) for
  long-running work.
- **Expression-based dynamic queries** — list endpoints build `OrderBy`/filter
  via reflection over allowed property names (a light Specification flavor).
- **Value normalization / anti-corruption** — the CPE resolver cleans messy nmap
  banners (`"6.6.1p1 Ubuntu 2ubuntu2.13"` → `6.6.1p1`) into canonical CPE URIs —
  an anti-corruption layer between the outside world's mess and our model.

---

## 7. Data model & persistence design

Core tables (all in one Postgres DB, one `AppDbContext`):

- **Scan** (`Id, RequestId*, TeamId, CreatedByUserId, Target, Status, …`) →
  **ScanResult** (port/protocol/service/state/product/version).
- **ScanRiskAssessment** (`Id, RequestId*, TeamId, CreatedByUserId, ScanId,
  Status, OverallRiskScore, …`) → **ScanRiskAssessmentFinding**
  (port/service/product/version/cpe/matchedCves/cvss/kevFlag/confidence).
- **CveCacheEntry** (`CpeUri+CveId` PK, cvss, cachedAt) — the local CVE cache.
- **Team** (= Clerk org), **User** (= Clerk user), **TeamMembership** (role) —
  the tenancy graph, JIT-mirrored from Clerk.
- **OutboxMessage** (type/message/status) — the outbox.
- **ServiceHeartbeat** (serviceName PK, lastSeenAt) — liveness.

`*` = unique index backing idempotency. `Status` values live in a shared
`Contracts.Constants.Status` (`Pending/Running/Completed/Failed`) used verbatim
by the Go service too. EF Core **code-first migrations** are the single schema
authority; the Go service targets the same physical tables with matching casing.

---

## 8. Security & multi-tenancy design

- **Tenant = Clerk Organization.** The JWT carries `org_id` + `org_role`; the
  active org is the tenant boundary for every request.
- **JIT identity sync** — `IdentitySyncService` upserts `User`, `Team`, and
  `TeamMembership` from the token on each request (no webhooks yet). It only
  overwrites fields when the incoming value is non-null (so a token missing a
  claim never clobbers stored data — a bug found and fixed during testing).
- **Structural isolation via EF Core global query filter** — every query on
  `Scan`/`ScanRiskAssessment` is automatically scoped to the current team. There
  are **no per-handler tenant checks**; isolation is a property of the data
  layer, which is far harder to forget than manual `WHERE TeamId = …`.
- **No-active-org → 403** — an authenticated request with no active org is
  rejected *before* it reaches a controller, because a null tenant would bypass
  the query filter (the filter's null-bypass is intended only for the workers,
  which use a `NullCurrentTeamAccessor`).
- **Creator attribution** — `CreatedByUserId` records who issued each scan /
  assessment; `createdByEmail` is surfaced for display.
- **Guardrail validation** — a risk assessment can only be created for a scan
  that exists, is `Completed`, and has results — enforced by a DB-backed
  FluentValidation rule.
- **Dev bypass** — a Development-only auth handler trusts `X-Dev-*` headers so the
  whole tenancy pipeline is testable offline without a live Clerk instance; it is
  environment-gated and never reachable in production.

---

## 9. The vulnerability-intelligence domain

The Go pipeline turns messy scan output into scored findings:

1. **CPE resolution** — map `product/version` to a canonical CPE URI using a
   curated vendor:product dictionary + version normalization, returning a
   **confidence** (dictionary hit + clean version = high; unknown product =
   low). *This is the dissertation's headline "novel contribution" — nmap banners
   are noisy and fuzzy-matching them is the hard part.*
2. **CVE lookup** — query NVD by CPE (cache-aside), extract CVE IDs + CVSS
   (v3.1 → v3.0 → v2 fallback).
3. **KEV flagging** — mark CVEs present in the live CISA KEV catalog.
4. **Scoring** — per finding = `CVSS × KEV-multiplier`, with a **heuristic tier**
   for CVE-less but risky services (telnet/ftp/rdp…). Overall score is weighted
   **70% worst-finding / 30% average** (a single critical service should
   dominate, not be diluted by many benign ones).
5. **Persist + expose** — findings written to the shared DB and returned by the
   API.

**Caveat captured in the model:** distro-patched software (e.g. Ubuntu-backported
Apache) can cause false positives in naive version matching — represented as the
confidence tier rather than hidden.

---

## 10. Testing strategy

- **.NET unit tests** cover the `Services` layer (business logic) — the create
  flow, idempotency race, validation, and the nmap parser (verified against real
  `nmap -sV` XML).
- **Go unit tests** cover the two algorithmic cores: the **CPE resolver** and the
  **scorer** (the parts most worth pinning down).
- **End-to-end verification** was done live against the Docker stack: scan →
  assessment → real CVE findings (PostgreSQL → 20 CVEs, CVSS 9.8), cross-tenant
  isolation, the no-org 403, JIT sync, and the connectivity-dot failover.

Rationale: unit-test the algorithms and the business rules; prove the wiring with
end-to-end runs rather than mocking Kafka/Postgres/nmap.

---

## 11. Key trade-offs & limitations

- **Shared DB over database-per-service** — chosen for simplicity + read-your-
  writes; the honest cost is that the Go service is coupled to the EF-managed
  schema.
- **Single-broker Kafka + single DB instance** — sufficient throughput for the
  target load, but a single point of failure (availability ceiling, not a
  throughput ceiling). Stated explicitly rather than claimed away.
- **JIT identity sync (no webhooks)** — the DB roster reflects only users who have
  called the API; removals/other members need Clerk webhooks (future work).
- **On-demand NVD (not a full mirror)** — first assessment of a new service makes
  a network call; a full offline NVD sync + rate-limit/backoff is future work.
- **Dashboard stats are client-side over a 25-row page** — exact aggregates need a
  `/dashboard/summary` endpoint (future work).
- **Compute scales near-linearly (replicas); the data layer is the real ceiling**
  — the honest scaling story.

See `PROJECT_STATUS.md` for the concrete done / to-do checklist.

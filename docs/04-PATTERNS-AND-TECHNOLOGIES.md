# Vantage — Patterns and Technologies

Every architectural pattern, integration pattern, tactical pattern and library
used in this project — what it is, where it appears, and **why it was chosen
over the alternative**. A pattern with no justification is cargo cult; each
entry here states the problem it solves and what it costs.

---

## Part I — Architectural patterns

### 1. Clean Architecture / Onion

**Where:** both the .NET side (`Domain → Application → Infrastructure/WebAPI`)
and the Go side (`domain → application → adapters`).

**The rule:** dependencies point inward. `Domain` and `Application` reference no
framework, no database, no HTTP.

**Why:** the business rules of this system — what makes a scan valid, when an
assessment may be requested, how risk aggregates — are the parts worth
protecting. Tying them to EF Core or ASP.NET means every future change to those
libraries becomes a change to business logic. Inverting the dependency means
Postgres, Kafka and nmap are all replaceable adapters.

**Concretely:** `IScanRepository` lives in `Domain`; its EF Core implementation
lives in `Infrastructure.Persistence`. `Application` depends on the interface
and has never heard of Npgsql.

**Cost:** more projects, more interfaces, and indirection that is genuine
overhead on a small codebase. It pays off exactly when something needs replacing
— which the polyglot split then proves.

### 2. Ports and Adapters (Hexagonal)

**Where:** most explicitly in the Go service. `internal/domain` declares
interfaces — `StatusWriter`, `FindingsWriter`, `CPEResolver`, `CVELookup`,
`KEVCatalog`, `Scorer` — and every other package implements one.

**Why:** the assessment pipeline is a sequence of independently substitutable
steps. Expressing each as a port means the orchestration logic can be read and
tested without a database, a network, or Kafka.

**Enforced by:** compile-time assertions, `var _ domain.Scorer = (*Scorer)(nil)`
— a build error if an adapter drifts from its port.

### 3. Microservices with event choreography

**Where:** API → Kafka → two independent workers.

**Why choreography over orchestration:** there is no central coordinator. Each
service reacts to events and writes its own results. The workflow is two
independent single-step reactions with no ordering constraint between them and
nothing to compensate. An orchestrator would introduce a component that must be
deployed, monitored and made highly available, in exchange for coordination this
system does not need.

**When that flips:** the moment steps must be sequenced, or a failure must roll
back earlier work, orchestration (a saga coordinator) starts earning its cost.

### 4. Polyglot services

**Where:** Go for vulnerability intelligence, .NET for everything else.

**Why:** a forcing function for boundary integrity. Same-language services drift
into shared helpers and leaked domain types until the "services" are one
deployable in disguise. A different runtime makes that structurally impossible —
the Go service *cannot* import a C# class. The only things crossing are a JSON
message contract and a table shape, and if either were sloppy the split would
break loudly.

Go also genuinely fits: a lean, concurrent, network-and-data service with a
small footprint and fast start-up.

**Cost:** two toolchains, two dependency sets, two test runners, and duplicated
DTO definitions.

### 5. Shared database (over database-per-service)

**Where:** one PostgreSQL database, one `AppDbContext`; the Go service writes to
the same tables via pgx.

**Why:** read-your-writes consistency for the poll-based UX, structural symmetry
between the two workers, and no need for a result-return channel (a second
topic, a consumer in the API, and reconciliation).

**Cost, stated plainly:** the Go service is coupled to an EF-managed schema, and
a breaking migration must be coordinated across two codebases.

**Why the boundary is still real:** shared *storage* is not shared *code*. The
coupling is a published schema — the same category of coupling any two services
have to their message format.

### 6. Layered read/write separation (light CQRS)

**Where:** writes go through domain services → unit of work → outbox. Reads use
dedicated query methods (`GetPagedAsync`, `GetWithFindingsByIdAsync`,
`GetCreatedAtInRangeAsync`) with `AsNoTracking` projections and output caching.

**Why:** the two paths have different needs. Writes need transactions, change
tracking and invariants; reads need projection, paging and caching. Separating
them lets each be optimised without compromise.

**Not full CQRS:** no separate models, no separate stores, no event sourcing.
The separation is applied where it pays and stopped where it would only add
ceremony.

---

## Part II — Integration and distributed-systems patterns

### 7. Transactional Outbox

**The problem (dual write):** writing a row to Postgres and publishing to Kafka
are two systems with no shared transaction. If the insert commits and the
publish fails, the work is silently lost. If the publish succeeds and the insert
rolls back, a worker processes a scan that does not exist.

**The solution:** write the business row **and** an `OutboxMessage` in one
database transaction. A separate publisher reads pending rows and sends them.
Either both exist or neither does.

**Where:** `ScansService` and `ScanRiskAssessmentsService`; published by
`ProcessOutboxMessagesJob` every 2 seconds.

**Consequence:** delivery becomes **at-least-once**, never exactly-once — which
is precisely why every consumer downstream is idempotent.

### 8. Strategy — dispatch by message type

**Where:** `OutboxMessage.Type` selects an `IOutboxMessagePublisher`.

**Why:** the outbox began hard-coded to one message type. Adding risk
assessments would have meant a second table and a second job, or a `switch` that
grows forever. Instead one table and one job serve any number of message types,
each with its own publisher — including two *different transports*.

| Type | Transport | Reason |
|---|---|---|
| `ScanRequestMessage` | MassTransit | .NET → .NET; envelope is fine |
| `ScanRiskAssessmentRequestedMessage` | raw `Confluent.Kafka` | Go consumer reads plain JSON |

**The interoperability lesson:** MassTransit wraps payloads in its own envelope,
which a non-MassTransit consumer cannot parse. Discovering this required reading
MassTransit's source rather than its documentation. The Strategy pattern is what
made accommodating it a new class rather than a rewrite.

### 9. Atomic claim (competing consumers)

**The problem:** at-least-once delivery plus multiple replicas means the same
work can arrive twice, simultaneously.

**Two variants:**

*Outbox rows* — `FOR UPDATE SKIP LOCKED`:
```sql
UPDATE "OutboxMessages" SET "Status" = 'Processing'
WHERE "Id" IN (
  SELECT "Id" FROM "OutboxMessages" WHERE "Status" = 'Pending'
  ORDER BY "OccurredOnUtc" FOR UPDATE SKIP LOCKED LIMIT @batch)
RETURNING *;
```
`SKIP LOCKED` is the crux: a concurrent claimer **skips** locked rows rather
than blocking on them, so throughput scales with publishers instead of
serialising.

*Scan rows* — conditional update:
```csharp
.Where(s => s.Id == id && s.Status == Status.Pending)
.ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, Status.Running));
// rowsAffected > 0 ⇒ this caller won
```

**Why not read-then-write:** `SELECT` followed by `UPDATE` has a window between
them in which another worker can claim the same row. Making the read part of the
write closes it.

### 10. Idempotency keys

**Where:** `X-Idempotency-Key` on every create, stored as a unique `RequestId`.

**Why:** clients retry — on timeouts, flaky networks, double-clicks. Without a
key, a retry means a duplicate scan.

**Implemented twice over, deliberately:** the path checks for an existing row
*and* catches the unique-constraint violation. The check handles the common
case; the catch handles two genuinely simultaneous requests where both checks
pass before either insert commits.

### 11. Async request-reply with polling

**Where:** creates return `202 Accepted` + `Location` + id; clients poll `GET`.

**Why polling over websockets/SSE/callbacks:** it needs no persistent
connection, no push infrastructure, no reconnection logic; it survives page
reloads and proxies; and TanStack Query makes adaptive intervals trivial —
faster while work is in flight, stopping entirely once terminal.

**Cost:** slightly stale data between polls and some redundant requests. For
operations measured in seconds-to-minutes, that is the right trade.

### 12. Cache-aside

**Where:** the CVE cache and the HTTP output cache.

*CVE cache:* check `CveCacheEntries` (7-day TTL) → on miss call NVD → populate.
The second assessment of the same product makes **no network call at all**.

*Output cache:* list endpoints are cached and **evicted by tag** on create, so a
new scan appears immediately rather than after a TTL.

**Why cache-aside over write-through:** the cache is a read optimisation over a
third-party source we do not control. Write-through implies we own the writes;
we do not.

### 13. Client-side rate limiting with backoff and jitter

**Where:** the NVD client.

**Why:** NVD publishes a quota (≈5 requests/30s anonymous, 50/30s with a key)
and returns 429/403 when exceeded. Ignoring it means losing CVE data under load
— silently, because a failed lookup looks like "no vulnerabilities".

**How:** a monotonic next-slot clock spaces requests (concurrent assessments
queue rather than burst); failures retry up to four times with exponential
backoff **plus jitter** (jitter prevents retries from re-synchronising into a
second thundering herd), honouring `Retry-After` when present.

**Retry classification matters:** 429/403/5xx and transport errors retry; a
4xx does not — retrying a malformed request only burns quota. Every wait is
context-aware so a cancelled assessment stops sleeping immediately.

### 14. Graceful degradation

**Where:** NVD unreachable → log a warning, return what is available, complete
the assessment anyway.

**Why:** a vulnerability feed outage should reduce the *quality* of an answer,
not deny one. The alternative — failing the whole assessment — turns a
third-party's availability into ours.

### 15. Heartbeat / health check

**Where:** each worker upserts `ServiceHeartbeats` every 10 s
(`ON CONFLICT … DO UPDATE`); `/api/health` reports up if seen within 30 s.

**Why a database write rather than an HTTP ping:** it proves the worker is
running **and** can reach the database. A process that is alive but cannot
reach Postgres is useless, and an HTTP health check would report it healthy.

### 16. Backpressure

**Where:** the scan consumer's `PrefetchCount`, `ConcurrentMessageLimit`,
`ConcurrentConsumerLimit` and checkpoint interval.

**Why:** without bounds, a burst of requests pulls unbounded work into one
worker, which then exhausts memory or process handles running many concurrent
nmap invocations. Bounding pull is how a queue absorbs a spike instead of
transmitting it.

### 17. Two-tier timeout (graceful budget + hard backstop)

**Where:** nmap's `--host-timeout 600s` versus `Nmap:TimeoutSeconds` 660 s.

**Why they differ, and why the gap is deliberate:**

| | Mechanism | Result |
|---|---|---|
| `--host-timeout` | nmap self-limits, exits 0 with partial XML | **Completed** |
| Process kill | `Process.Kill(entireProcessTree)`, output discarded | **Failed** |

The graceful budget must fire **first** so partial results survive. Setting them
equal creates a race in which the kill can destroy results nmap was about to
emit. The backstop exists solely for nmap itself hanging past its own limit.

### 18. Webhook signature verification (Svix / HMAC)

**Where:** `SvixSignatureVerifier`, guarding `/api/webhooks/clerk`.

**Why:** the endpoint must be publicly reachable and unauthenticated — anyone
can POST to it. Without verification, an attacker could forge
`organization.deleted` and cascade-delete a tenant.

**Three defences:**
1. **HMAC-SHA256** over `{id}.{timestamp}.{raw body}` — proves the sender holds
   the shared secret and the body is unmodified.
2. **Constant-time comparison** (`FixedTimeEquals`) — a naive `==` returns early
   on the first differing byte, leaking, over many attempts, enough timing
   signal to forge a signature byte by byte.
3. **±5-minute timestamp window** — a validly-signed request captured off the
   wire cannot be replayed later.

**Fail closed:** with no secret configured the endpoint returns **503** rather
than accepting unverified events.

### 19. Just-in-time identity sync (+ webhooks)

**Where:** `IdentitySyncService` on every request; `ClerkWebhookService` for
events.

**Why both:** JIT is always current for the caller and cannot fail to be
delivered, but can only ever see the caller. Webhooks see everyone but are
eventually consistent. Neither alone gives a roster that is both immediately
correct and eventually complete.

**Detail that matters:** JIT only overwrites a field when the incoming value is
non-null, so a token missing a claim never blanks stored data.

---

## Part III — Tactical / code-level patterns

### 20. Repository

`IRepository<TEntity,TKey>` / `IReadOnlyRepository<>` plus specialised
interfaces. Abstracts persistence from application logic and keeps `Domain` free
of EF Core. The read-only split expresses intent — a query service cannot
accidentally mutate.

### 21. Unit of Work

`IUnitOfWork` owns the repositories, `SaveChangesAsync` and transaction control,
so a business operation spanning several tables (the row *and* its outbox
message) commits atomically. Without it, each repository would own its own
`SaveChanges` and atomicity would be impossible to express.

It also carries two capabilities that exist for hard-won reasons:

- **`IsUniqueConstraintViolation`** — translates Postgres SQLSTATE `23505` into
  a domain concept so `Application` can handle races without referencing Npgsql.
- **`Detach`** — after a failed `SaveChangesAsync`, the offending entity remains
  tracked in `Added` state and **poisons every later save in the same request**,
  which throws outside the original catch block. Detaching is what makes the
  race handling correct rather than merely present.

### 22. Result pattern

`Result` / `Result<T>` model success, validation failure and not-found as
**values**, not exceptions.

**Why:** a scan target failing validation is an expected outcome, not an
exceptional one. Exceptions for control flow are expensive, easy to swallow, and
obscure the fact that failure is part of the contract. Returning a `Result`
makes the failure path visible in the signature.

`ApiControllerBase.HandleFailure` maps results to HTTP in exactly one place.

### 23. Dependency injection with convention registration

Scrutor scans assemblies and registers by convention — classes ending in
`Service`, every `IValidator<>`, every `IRepository<>`.

**Why:** adding a service or validator requires no registration line, so the
most common way to break DI (forgetting to register) is designed out.

### 24. Options pattern

`IOptions<KafkaOptions>`, `ClerkOptions`, `AuthOptions`, `ClerkWebhookOptions`,
`NmapOptions`. Configuration is bound to typed objects, overridable by
environment variables using the `__` convention.

Two entries validate at start-up rather than on first use:
`Clerk:Authority` is required when `Auth:Mode=Clerk` (`ValidateOnStart`), and
`DevHeaders` outside Development throws. **Failing at boot beats failing on a
user's first request.**

### 25. DTO + mapper

`Contracts` DTOs never expose entities; static extension mappers (`ToDto`,
`ToGetResponse`, `ToEntity`, `ToOutboxMessage`) convert.

**Why:** returning entities leaks internal columns and turns any schema change
into an API-breaking change.

### 26. Validation orchestrator

A thin service resolves every `IValidator<T>` for a request and aggregates
failures into the `Result` error model — so a caller sees *all* problems at
once, and validators can inject the unit of work for DB-backed rules (the
risk-assessment guardrail: the scan must exist, be `Completed`, and have
results).

### 27. Factory

`ErrorFactory` centralises error construction so codes and messages stay
consistent rather than being spelled slightly differently in each service.

### 28. Middleware pipeline

A global exception handler and `IdentityResolutionMiddleware`.

**Order is load-bearing:** identity resolution must run **after** authentication
(it needs claims) and **before** controllers (its 403 must pre-empt any handler
that would query with a null tenant).

### 29. Global query filter (structural multi-tenancy)

An EF Core filter scopes every `Scan`/`ScanRiskAssessment` query to the current
team.

**Why this over per-handler checks:** a `WHERE TeamId = …` in every handler is
correct until someone forgets once — and that single omission is a cross-tenant
data leak. A filter in the data layer is applied by default; a new endpoint is
isolated without anyone remembering to isolate it.

**The null-bypass** exists so background workers (which have no user) can
operate — which is exactly why an authenticated web request with no active
organisation must be rejected *before* it reaches a controller.

### 30. Background services

.NET `IHostedService`/`BackgroundService` for the heartbeat and the Quartz job;
Go goroutines for the consumer loop, heartbeat and KEV refresh. Long-running
work that must not be tied to a request lifetime.

### 31. Expression-based dynamic queries

List endpoints build `OrderBy`/filters via reflection over an **allow-list** of
property names — a light Specification flavour.

**Security note:** the allow-list is the point. Passing a user-supplied string
straight into a dynamic query is an injection surface; validating against known
properties closes it.

### 32. Anti-corruption layer

The CPE resolver, translating the outside world's messy reality
(`6.6.1p1 Ubuntu 2ubuntu2.13`, `8.3.0 - 8.3.7`,
`Apache Tomcat/Coyote JSP engine`) into canonical CPE URIs.

**Why it is the right name:** without it, banner noise would propagate into the
domain model and every downstream consumer would need to cope with it. Instead
the mess is normalised once, at the boundary, and the domain sees only canonical
identifiers — plus a **confidence** value that carries the uncertainty honestly
rather than discarding it.

### 33. Confidence scoring as an abstention signal

Rather than guessing silently, the resolver reports how much it trusts each
match (0.9 dictionary + version, down to 0.2 fallback without version).

**Why this is a pattern and not a field:** it converts an unavoidable
false-positive problem into a *tunable* one. The evaluation harness shows that
thresholding at 0.5 removes every false positive — the score is a working
abstention mechanism, not decoration.

---

## Part IV — Technologies

### Backend runtimes

| Technology | Where | Why this and not the alternative |
|---|---|---|
| **.NET 10 / C#** | API, scan worker | Mature EF Core, first-class DI and hosting, excellent async, strong typing. The API and scan worker share a domain, so one language for both removes friction. |
| **Go** | vulnintel-service | Proves the boundary is real (see §4). Idiomatic for a lean concurrent data/network service; small footprint, fast cold start. |

### Messaging

| Technology | Why |
|---|---|
| **Apache Kafka (KRaft)** | A durable, **replayable** log — not just a queue. Consumer groups give competing consumers and rebalancing for free. KRaft drops the ZooKeeper dependency, halving local footprint. Replayability matters: reprocessing is re-reading, not resurrecting lost messages. |
| **MassTransit** | Producer/consumer abstraction for the .NET↔.NET scan flow: serialisation, endpoint configuration, concurrency limits, checkpointing. |
| **`Confluent.Kafka` (raw)** | The risk-assessment topic, because MassTransit's envelope is unreadable by the Go consumer. |
| **`segmentio/kafka-go`** | Go consumer + heartbeat. Lightweight, dependency-free. |

### Persistence

| Technology | Why |
|---|---|
| **PostgreSQL** | Supports the exact primitives this design leans on: `FOR UPDATE SKIP LOCKED` (atomic claim), unique constraints (idempotency), `ON CONFLICT` upserts (heartbeats, cache), and SQLSTATE codes precise enough to distinguish a unique violation. |
| **EF Core (Npgsql)** | Single schema authority via code-first migrations; global query filters give structural multi-tenancy; one `DbContext` keeps tables joinable. |
| **`pgx/v5`** | High-performance native Go driver. The Go service issues a handful of parameterised statements — an ORM would be overhead. |

### Supporting libraries

| Technology | Why |
|---|---|
| **Quartz.NET** | Schedules the outbox publisher every 2 s. Misfire handling, DI integration and hosted-service lifecycle are solved problems; a hand-rolled timer loop would reimplement them badly. |
| **FluentValidation** | Declarative, composable validation; validators are auto-discovered and can inject the unit of work for DB-backed rules. |
| **Newtonsoft.Json** | Outbox payload (de)serialisation with explicit camelCase and reference-loop handling, matching exactly what the Go consumer expects. |
| **Serilog** | Structured logging, console + rolling file, configured from `appsettings`. |
| **Scrutor** | Assembly-scanning DI registration (§23). |
| **xunit + Moq** | .NET testing. |

### External intelligence

| Source | Why |
|---|---|
| **`nmap` with `-sV`** | The de-facto scanner. `-sV` is **required**, not optional: without service/version detection there is no product+version, hence no CPE, hence no CVE matching. |
| **NVD 2.0 REST API** | Authoritative CVE and CVSS data, queryable by CPE. |
| **CISA KEV catalogue** | The list of *known-exploited* vulnerabilities. Severity says how bad a flaw could be; KEV says it is being used right now — which is why it weights the score. |

### Auth and frontend

| Technology | Why |
|---|---|
| **Clerk (Organizations)** | Auth, orgs, roles, invitations and account UI out of the box. Organizations map 1:1 onto the team tenant model, so member management and an org switcher come free. Building this is weeks of work with real security risk. |
| **`@clerk/react` (Core 3)** | Current SDK. Core 2 (`@clerk/clerk-react`) is deprecated; Core 3 renamed `<SignedIn>`/`<SignedOut>` to `<Show when>` and changed appearance variables. |
| **React + Vite + TypeScript** | Fast dev loop; TS guards the API contract. |
| **Tailwind + shadcn/ui** | Utility styling plus accessible copy-in primitives — components live in the repo and are editable, with no library upgrade treadmill. |
| **TanStack Query** | Server-state caching, adaptive polling (the async UX depends on it), and cache invalidation on org switch. |
| **recharts** | Charting engine behind shadcn's chart primitives. |
| **react-router, sonner** | Routing and toasts. |
| **`@clerk/themes`** | Dark-themes Clerk widgets to match the app. |
| **Playwright** | End-to-end browser verification, with `@clerk/testing` tokens to bypass bot protection in automation. |

### Orchestration

| Technology | Why |
|---|---|
| **Docker Compose** | One command brings up the database, broker, API, both workers, the portal and Kafka UI. Kubernetes was explicitly out of scope. |

---

## Part V — Patterns deliberately **not** used

Naming what was rejected is as informative as naming what was adopted.

| Pattern | Why not |
|---|---|
| **Event sourcing** | Current state is what the UI needs; a full event log would add replay and projection machinery for an audit benefit already met by status timestamps and attribution. |
| **Full CQRS with separate stores** | Read and write volumes are comparable and both are small. Separate stores would add synchronisation and eventual consistency for no measured gain. |
| **Saga / process manager** | No multi-step distributed transaction exists to coordinate or compensate (§3). |
| **Database-per-service** | Deliberate trade documented in §5 — consistency and symmetry over independence. |
| **API gateway / BFF** | One SPA and one API. A gateway would add a hop and a deployment for no aggregation benefit. |
| **gRPC between services** | Services communicate by *events*, not calls. A synchronous RPC channel would reintroduce exactly the coupling the architecture removes. |
| **Distributed cache (Redis)** | The CVE cache is durable, shared data, so it belongs in Postgres. Output caching is per-instance and cheap to rebuild. Redis would add a component to operate for neither. |
| **Kubernetes** | Out of scope for this project. |

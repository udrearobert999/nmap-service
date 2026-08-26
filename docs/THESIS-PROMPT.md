# Thesis generation — proposed structure and skywork.ai prompt

Two parts:

- **Part A** — the proposed thesis structure, with per-section word budgets that
  add up to roughly 65–70 pages, and every figure/table placeholder.
- **Part B** — the prompt to paste into skywork.ai. It is self-contained: it
  carries every verified fact about Vantage so the generator cannot invent them.

Before running it, set the language on the `LANGUAGE:` line and attach:
1. `Cosmin-Gabriel Sulita - Master's Thesis.pdf`
2. `alexandrescu_cristian_inginerie_software_2020.pdf`
3. `Sistem de mesagerie cu criptare integrală.docx`
4. `docs/01-HIGH-LEVEL-ARCHITECTURE.md`, `02-LOW-LEVEL-ARCHITECTURE.md`,
   `03-FUNCTIONALITIES.md`, `04-PATTERNS-AND-TECHNOLOGIES.md`

---

# Part A — Proposed structure

Front matter (title page, declaration, theme sheet, supervisor's report,
acknowledgements, contents, list of figures, list of tables) is **written by
you** and is not generated. The document begins at Chapter 1.

| # | Chapter | Words | ≈ Pages |
|---|---|---|---|
| 1 | Introduction | 2,200 | 6 |
| 2 | Network scanning and service fingerprinting | 2,600 | 7 |
| 3 | Vulnerability intelligence: CVE, CPE, CVSS, KEV | 3,000 | 8 |
| 4 | Architectural and design patterns for distributed systems | 4,800 | 13 |
| 5 | Multi-tenancy, identity and application security | 2,700 | 7 |
| 6 | Technologies used | 2,200 | 6 |
| 7 | Analysis and design of the Vantage system | 3,000 | 9 |
| 8 | Implementation | 4,200 | 11 |
| 9 | Testing, validation and experimental evaluation | 2,600 | 7 |
| 10 | Conclusions | 1,100 | 3 |
| — | Bibliography and web references | — | 3 |
| | **Total** | **~28,400** | **~70** |

Chapters 2–6 are **pure theory and total ~41 pages — about 58% of the body**,
which matches the balance in both Craiova exemplars.

### Chapter 1 — Introduction (2,200 w)
- 1.1 Purpose — 250
- 1.2 Motivation — 300 (first person, as in both exemplars)
- 1.3 Context and problem statement — 300
- 1.4 Objectives of the project — 200
- 1.5 Development and design steps — 450 (analysis, prototyping, design,
  implementation, testing, evaluation)
- 1.6 Functional requirements — 350 (numbered `RF-01…`, grouped by user class)
- 1.7 Non-functional requirements — 250 (numbered `RNF-01…`)
- 1.8 Personal contributions — 150
- 1.9 Structure of the thesis — 150

`[FIGURE 1.1]` Context diagram — operator, Vantage, scanned infrastructure, NVD/KEV, Clerk.
`[TABLE 1.1]` Functional requirements. `[TABLE 1.2]` Non-functional requirements.

### Chapter 2 — Network scanning and service fingerprinting (2,600 w)
2.1 The reconnaissance phase · 2.2 TCP/IP and port states (open, closed,
filtered, unfiltered, `tcpwrapped`) · 2.3 Scan techniques (SYN, connect, UDP,
`-Pn`) · 2.4 Service and version detection (`-sV`, probe database, banner
grabbing) · 2.5 Timing, retries and host timeouts · 2.6 Limitations: filtered
networks, distribution-backported versions, false negatives · 2.7 Ethical and
legal considerations.

`[FIGURE 2.1]` TCP three-way handshake vs SYN scan.
`[FIGURE 2.2]` Port state decision tree. `[FIGURE 2.3]` Annotated `-sV` output.
`[TABLE 2.1]` Comparison of scan techniques.

### Chapter 3 — Vulnerability intelligence (3,000 w)
3.1 Disclosure lifecycle · 3.2 CVE identifiers · 3.3 The NVD · 3.4 **CPE** —
2.3 URI grammar, dictionary, why vendor ≠ product · 3.5 **CVSS** v2/v3.0/v3.1
base metrics and vectors · 3.6 **CISA KEV** and exploit maturity · 3.7 The
matching problem: fuzzy banners, version normalisation, false positives ·
3.8 Precision, recall and F1 as evaluation measures · 3.9 Prior art in automated
vulnerability correlation.

`[FIGURE 3.1]` Disclosure lifecycle. `[FIGURE 3.2]` Anatomy of a CPE 2.3 URI.
`[FIGURE 3.3]` CVSS v3.1 base metric groups. `[FIGURE 3.4]` Banner → CPE → CVE pipeline.
`[TABLE 3.1]` CVSS severity bands. `[TABLE 3.2]` Confusion matrix definitions.

### Chapter 4 — Architectural and design patterns (4,800 w) ← the theory core
4.1 Monolith vs microservices · 4.2 **Clean Architecture** and the dependency
rule · 4.3 **Ports and Adapters** · 4.4 Synchronous vs asynchronous integration
· 4.5 Message brokers and the log abstraction; delivery semantics · 4.6
**Transactional Outbox** and the dual-write problem · 4.7 **Strategy** for
dispatch by type · 4.8 **Competing consumers and atomic claim**
(`FOR UPDATE SKIP LOCKED`) · 4.9 **Idempotency** · 4.10 **Async request–reply
with polling** · 4.11 **Choreography vs orchestration** · 4.12 **Repository and
Unit of Work** · 4.13 **Result pattern** vs exceptions · 4.14 **Cache-aside** ·
4.15 **Rate limiting, exponential backoff and jitter** · 4.16 **Circuit breaking
and graceful degradation** · 4.17 **Health checks and heartbeats** · 4.18
**Backpressure** · 4.19 **Anti-corruption layer** · 4.20 **CQRS** (and why only
partially adopted) · 4.21 Dependency injection and the Options pattern.

Every section: definition → problem it solves → how it works → trade-offs →
alternatives rejected.

`[FIGURE 4.1]` Clean Architecture rings. `[FIGURE 4.2]` Hexagonal ports/adapters.
`[FIGURE 4.3]` Dual-write failure. `[FIGURE 4.4]` Outbox sequence.
`[FIGURE 4.5]` `SKIP LOCKED` with concurrent workers. `[FIGURE 4.6]` Async request–reply.
`[FIGURE 4.7]` Choreography vs orchestration. `[FIGURE 4.8]` Cache-aside flow.
`[FIGURE 4.9]` Backoff with and without jitter.
`[TABLE 4.1]` Delivery semantics. `[TABLE 4.2]` Pattern → problem → trade-off summary.

### Chapter 5 — Multi-tenancy, identity and application security (2,700 w)
5.1 Tenancy models (silo, bridge, pool) · 5.2 Isolation strategies and the
forgotten-`WHERE` failure mode · 5.3 Row-level isolation via query filters ·
5.4 OAuth 2.0 / OIDC / JWT structure and validation · 5.5 Identity providers and
Clerk Organizations · 5.6 Session-token claim shapes · 5.7 Identity
synchronisation: JIT vs webhooks · 5.8 **Webhook security**: HMAC, replay
windows, constant-time comparison, timing attacks · 5.9 Secrets, injection
surfaces and least privilege.

`[FIGURE 5.1]` Tenancy models. `[FIGURE 5.2]` Query-filter isolation.
`[FIGURE 5.3]` JWT structure. `[FIGURE 5.4]` Webhook signature verification.
`[TABLE 5.1]` Tenancy model comparison.

### Chapter 6 — Technologies used (2,200 w)
6.1 .NET 10 / C# · 6.2 Go · 6.3 PostgreSQL and the primitives relied upon ·
6.4 EF Core and Npgsql · 6.5 Apache Kafka in KRaft mode · 6.6 MassTransit and
Confluent.Kafka · 6.7 Quartz.NET · 6.8 nmap · 6.9 React, Vite, TypeScript,
Tailwind, shadcn/ui, TanStack Query, recharts · 6.10 Clerk · 6.11 Docker and
Docker Compose · 6.12 Justification of each choice against alternatives.

`[FIGURE 6.1]` Technology stack by layer. `[TABLE 6.1]` Technology → role → alternatives → rationale.

### Chapter 7 — Analysis and design of the Vantage system (3,000 w)
Follows the IEEE-830 shape used by Alexandrescu:
7.1 Introduction (7.1.1 Motivation, 7.1.2 Scope, 7.1.3 Definitions, acronyms and
abbreviations) · 7.2 General description (7.2.1 Product perspective, 7.2.2
Product functions, 7.2.3 User characteristics, 7.2.4 Constraints, 7.2.5
Assumptions and dependencies) · 7.3 Specific requirements (7.3.1 External
interfaces — user, hardware, software, communication; 7.3.2 Functional
requirements by user class) · 7.4 System architecture · 7.5 Data model ·
7.6 Use cases (enumerated, each with actor, precondition, main flow,
alternative flows, postcondition).

`[FIGURE 7.1]` High-level architecture. `[FIGURE 7.2]` Deployment diagram.
`[FIGURE 7.3]` Use-case diagram. `[FIGURE 7.4]` ER diagram.
`[FIGURE 7.5]` Scan sequence diagram. `[FIGURE 7.6]` Assessment sequence diagram.
`[FIGURE 7.7]` Scan state machine.
`[TABLE 7.1]` Definitions and acronyms. `[TABLE 7.2]` Use-case index. `[TABLE 7.3]` Entities.

### Chapter 8 — Implementation (4,200 w)
8.1 Solution structure and layer mapping · 8.2 REST API (controllers, middleware
pipeline, `Result` handling, output caching) · 8.3 Persistence (DbContext,
configurations, migrations, repositories, Unit of Work, query filter) ·
8.4 Outbox and publishers · 8.5 Scan worker (consumer, nmap runner, argument
construction, two-tier timeout, XML parser) · 8.6 vulnintel-service — 8.6.1
structure and ports, 8.6.2 CPE resolver, 8.6.3 CVE lookup and cache, 8.6.4 NVD
client with rate limiting and retry, 8.6.5 KEV, 8.6.6 scorer · 8.7 Authentication
and identity (auth modes, claim resolution, JIT sync) · 8.8 Clerk webhooks
(verification, handlers, delete semantics) · 8.9 Dashboard aggregation ·
8.10 Portal · 8.11 Docker Compose orchestration.

`[FIGURE 8.1]` Project dependency graph. `[FIGURE 8.2]` Middleware pipeline.
`[FIGURE 8.3]` Outbox publisher class diagram. `[FIGURE 8.4]` nmap timeout timeline.
`[FIGURE 8.5]` Go package/port diagram. `[FIGURE 8.6]` CPE resolution flowchart.
`[FIGURE 8.7]` NVD rate limiter and retry. `[FIGURE 8.8–8.13]` Portal screenshots:
dashboard, scans list, scan detail with findings, diff page, status page, sign-in.
`[FIGURE 8.14]` Compose topology.
`[TABLE 8.1]` API endpoints. `[TABLE 8.2]` Configuration variables. `[TABLE 8.3]` CPE confidence tiers.

### Chapter 9 — Testing, validation and experimental evaluation (2,600 w)
9.1 Strategy · 9.2 Unit tests (44 .NET, Go) · 9.3 Security-focused tests
(signature verification, replay, tampering) · 9.4 End-to-end validation ·
9.5 **Experimental evaluation of the CPE resolver** — 9.5.1 corpus and labelling,
9.5.2 baseline, 9.5.3 metric definitions, 9.5.4 results, 9.5.5 analysis,
9.5.6 defects discovered, 9.5.7 threats to validity · 9.6 Performance
observations.

`[FIGURE 9.1]` Test pyramid. `[FIGURE 9.2]` Precision/recall vs threshold.
`[FIGURE 9.3]` Baseline vs dictionary F1 bar chart.
`[TABLE 9.1]` Test suites. `[TABLE 9.2]` Evaluation corpus composition.
`[TABLE 9.3]` **Evaluation results.** `[TABLE 9.4]` Defects found by the harness.

### Chapter 10 — Conclusions (1,100 w)
10.1 Results achieved · 10.2 Fulfilment of objectives · 10.3 Limitations ·
10.4 Future work · 10.5 Personal reflection.

`[TABLE 10.1]` Objectives vs outcomes.

---

# Part B — The prompt for skywork.ai

Copy everything below the line.

---

```
ROLE
You are an academic technical writer producing a Master's thesis in software
engineering for the Faculty of Automatics, Computers and Electronics, University
of Craiova.

LANGUAGE: English
(Change to "Romanian" if you want the thesis in Romanian. If Romanian, use
Romanian technical terminology with the English term in parentheses on first
use, e.g. "tipar de proiectare (design pattern)".)

TASK
Write the complete body of a Master's thesis about the software project
"Vantage", described in the PROJECT FACTS section below. Follow the CHAPTER PLAN
exactly.

START AT CHAPTER 1 (INTRODUCTION). Do NOT generate the title page, originality
declaration, theme sheet, supervisor's report, acknowledgements, table of
contents, list of figures or list of tables — those are written separately.

ATTACHED REFERENCE DOCUMENTS
Three example theses are attached. Use them ONLY as models for structure, tone,
depth, section numbering and academic register — never copy their content, and
never write about their subject matter (augmented reality, web auditing,
encrypted messaging).
- Sulita — iOS Augmented Reality Application (same faculty, English)
- Alexandrescu — Auditor Scalabil de Securitate pentru Aplicatii Web (same
  faculty, same specialisation; imitate its IEEE-830 requirements chapter and
  its heavy theory-to-implementation ratio)
- Sistem de mesagerie cu criptare integrala (imitate its per-component
  implementation depth and its enumerated use cases)

Four project documents are also attached (01-HIGH-LEVEL-ARCHITECTURE.md,
02-LOW-LEVEL-ARCHITECTURE.md, 03-FUNCTIONALITIES.md,
04-PATTERNS-AND-TECHNOLOGIES.md). These are the authoritative description of the
system. Everything you write about Vantage must be consistent with them and with
the PROJECT FACTS below.

LENGTH — THIS IS A HARD REQUIREMENT
Minimum 50 pages of body text; target 65-70 pages. Approximately 28,000 words.
Word budgets per chapter are given in the CHAPTER PLAN and must be respected
within +/-15%. Do not summarise. Do not compress. If a section feels short,
develop it with definitions, worked explanations, comparisons and trade-off
analysis rather than padding with repetition.

Theory (chapters 2-6) must account for roughly 55-60% of the body. This ratio
matches the attached exemplars and is expected by the faculty.

STYLE RULES
- Formal academic register, third person, past tense for work performed.
  Exception: sections 1.2 (Motivation) and 10.5 (Personal reflection) are
  first person, as in both exemplars.
- Every technical term is defined at first use.
- Every pattern in chapter 4 follows the same five-part shape: definition; the
  problem it solves; how it works; trade-offs and costs; alternatives that were
  considered and rejected.
- Explain WHY, not only WHAT. A design decision stated without its rationale and
  its cost is incomplete.
- Numbered headings to at most four levels (e.g. 8.6.4).
- Use footnotes for web sources, following the exemplars' style.
- Code listings: short and illustrative only (5-20 lines), each with a caption
  "Listing X.Y". Never paste large files.
- Prefer tables for comparisons and enumerations.

FIGURE AND TABLE PLACEHOLDERS
Insert every figure listed in the CHAPTER PLAN at the point in the text where it
is referenced. Use exactly this format, on its own lines:

    [FIGURE 4.4: Sequence of the transactional outbox pattern]
    (Description for the author: show the API writing the business row and the
    outbox row inside one transaction, the Quartz job claiming pending rows every
    2 seconds, publishing to Kafka, and marking them processed. Show the failure
    path where publishing fails and the row stays pending for retry.)
    Figure 4.4 - Sequence of the transactional outbox pattern

That is: the bracketed placeholder, then a parenthesised description of what the
image must show so the author can produce it, then the final caption line.

Tables use the same idea but must be FULLY POPULATED with real content, not
placeholders:

    Table 9.3 - CPE resolution evaluation results
    | resolver | threshold | precision | recall | F1 |
    |---|---|---|---|---|
    | ... actual values from PROJECT FACTS ...

Always refer to figures and tables in the body text ("as shown in Figure 4.4").

ACCURACY RULES - CRITICAL
- Use ONLY the facts in PROJECT FACTS and the attached project documents when
  writing about Vantage. Do not invent features, metrics, benchmarks, user
  numbers, performance figures or deployment details.
- The ONLY quantitative results about Vantage are the CPE evaluation results in
  PROJECT FACTS. Do not fabricate any other measurement.
- For general theory (chapters 2-6) you may draw on established public knowledge,
  but cite real, verifiable sources. Never invent a citation, author, DOI or
  page number. If unsure of a source, describe the concept without attributing it.
- Where the project has limitations, state them honestly. The exemplars do this
  and it is expected.
- Mark anything you are unsure about as [TO VERIFY] rather than guessing.

CHAPTER PLAN
(Reproduce the "Part A" structure here in full — chapters 1 to 10 with their
subsection lists, word budgets and figure/table placeholders.)

PROJECT FACTS - AUTHORITATIVE

Name and purpose
Vantage is a multi-tenant network security platform. It answers two questions
about an organisation's infrastructure: what is exposed, and how dangerous it is.
It discovers reachable services by scanning, then correlates them against public
vulnerability intelligence to produce scored findings.

Central thesis
Decoupling network scanning from vulnerability correlation into independent,
event-driven services. The two workloads have different characteristics:
scanning is I/O-bound, slow and network-facing; correlation is data-bound and
depends on external feeds that evolve independently. Coupling them means the
slowest, least reliable part governs the whole.

Services (four)
1. REST API - .NET 10 / C#. Owns lifecycle state, authentication, tenancy,
   validation, and publishes work. Performs no slow work itself. Port 8080.
2. Scan worker - .NET 10. Consumes scan requests, executes nmap, parses XML,
   writes results.
3. vulnintel-service - Go. Consumes assessment requests, resolves CPEs, looks up
   CVEs, flags KEV, scores findings. Port 8090.
4. Portal - React 18 + Vite 5 + TypeScript SPA. Port 5173.

Infrastructure: PostgreSQL (single shared database, one EF Core AppDbContext,
15 migrations), Apache Kafka in KRaft mode (single broker), Kafka UI, Clerk as
external identity provider. Eight containers via one Docker Compose file.

Why polyglot
Go for the vulnerability service is a deliberate forcing function for boundary
integrity: it cannot import a C# class, so the only coupling is a Kafka JSON
message contract and a database table shape. Go also suits a lean, concurrent
data/network service.

Shared database trade-off
One database rather than database-per-service. Benefits: read-your-writes
consistency for the polling UX; structural symmetry between the two workers; no
need for a result-return topic and consumer. Cost: the Go service is coupled to
an EF-managed schema, and breaking migrations must be coordinated. Stated
explicitly as a documented trade-off.

Asynchronous flow (identical for both pipelines)
POST -> API writes business row AND outbox row in ONE transaction -> returns 202
Accepted with Location header -> Quartz job every 2 seconds claims pending outbox
rows and publishes to Kafka -> worker consumes, atomically claims the work
(Pending -> Running), performs it, writes results, marks Completed -> client polls
GET for status.

Transactional outbox
Solves the dual-write problem. Claim SQL uses
"UPDATE ... WHERE Status='Pending' ... FOR UPDATE SKIP LOCKED ... RETURNING *".
SKIP LOCKED lets concurrent claimers skip locked rows instead of blocking.
Delivery is at-least-once, which is why every consumer is idempotent.

Dispatch by type (Strategy pattern)
OutboxMessage.Type selects an IOutboxMessagePublisher:
- ScanRequestMessage -> MassTransit -> scan-requests-topic
- ScanRiskAssessmentRequestedMessage -> raw Confluent.Kafka ->
  scan-risk-assessment-requests-topic
Two transports because MassTransit wraps payloads in its own envelope, which the
Go consumer (plain JSON) cannot parse. A genuine polyglot interoperability
constraint.

Atomic claim for scans
ExecuteUpdate with "WHERE Id = @id AND Status = 'Pending'" setting Status =
'Running'; only the caller whose rowsAffected > 0 proceeds. Prevents double
processing on Kafka redelivery.

Idempotency
X-Idempotency-Key header stored as a unique RequestId. The create path both
checks for an existing row and catches the unique-constraint violation, so two
simultaneous identical requests collapse to one.

Backpressure
Scan consumer sets PrefetchCount (2x message limit), ConcurrentMessageLimit,
ConcurrentConsumerLimit, and a 5-second checkpoint interval.

nmap invocation
nmap -Pn -sV -T4 --max-retries 1 --host-timeout 600s -oX - <target>
Two-tier timeout, deliberately unequal:
- --host-timeout 600s: nmap self-limits, exits 0 with partial XML -> Completed
- Nmap:TimeoutSeconds = 660s: hard Process.Kill(entireProcessTree), output
  discarded -> Failed
The graceful budget must fire first so partial results survive; the kill is a
backstop for nmap itself hanging. Failure is judged by exit code and empty
output, not by stderr being non-empty, because nmap writes non-fatal warnings
(including its own host-timeout notice) to stderr.

CPE resolution (the anti-corruption layer)
Resolve(product, version) -> {URI, Confidence}
1. Lowercase and trim product; look up a curated vendor:product dictionary
   (openssh -> openbsd:openssh, apache httpd -> apache:http_server,
   mysql -> oracle:mysql, postgresql -> postgresql:postgresql,
   apache tomcat/coyote jsp engine -> apache:tomcat, and others).
   The dictionary exists because the vendor is frequently NOT the product name.
2. Normalise version with regex [0-9]+(\.[0-9]+)*([a-z][0-9a-z]*)? which turns
   "6.6.1p1 Ubuntu 2ubuntu2.13" into "6.6.1p1" and "8.3.0 - 8.3.7" into "8.3.0".
3. Emit cpe:2.3:a:{vendor}:{product}:{version}:*:*:*:*:*:*:*
4. Unknown product falls back to its first token as both vendor and product, at
   low confidence.
Confidence tiers: dictionary hit + version 0.9; dictionary hit, no version 0.6;
fallback + version 0.4; fallback, no version 0.2.

CVE lookup (cache-aside)
Check CveCacheEntries (7-day TTL) -> on miss call NVD 2.0 REST API -> populate.
CVSS taken from v3.1, falling back to v3.0, then v2. If NVD is unreachable, log
and return what is available; the assessment still completes (graceful
degradation).

NVD client hardening
Monotonic next-slot rate limiter: approximately 1 request per 6.5 seconds
anonymously, 1 per 0.7 seconds with NVD_API_KEY. Retries up to 4 attempts with
exponential backoff plus jitter, honouring Retry-After. Retries 429, 403, 5xx
and transport errors; a 4xx fails immediately rather than burning quota. All
waits are context-aware.

Scoring
finding_score = min(base * kev_multiplier, 10)
- base = the CVE's CVSS score; if there are no CVEs, a heuristic by service:
  telnet 7.0, rdp/ms-wbt-server 6.0, vnc 6.0, ftp 5.0, smb/netbios-ssn 5.0,
  otherwise 0.0
- kev_multiplier = 1.15 when the CVE appears in the CISA KEV catalogue
overall = min(0.7 * worst + 0.3 * average, 10)
The 70/30 weighting means one critical exposure dominates the headline number
while breadth still moves it.

Authentication
Auth:Mode selects the scheme independently of ASPNETCORE_ENVIRONMENT:
- DevHeaders: trusts X-Dev-Subject / X-Dev-Org / X-Dev-Role / X-Dev-Email;
  refuses to start outside Development
- Clerk: validates real Clerk JWTs against Clerk:Authority; fails fast at
  start-up if the authority is missing
Decoupling these two was necessary because previously enabling real JWT
validation also disabled Swagger and forced HTTPS redirection, which is why it
had never been exercised.

Clerk v2 session token
Nests the active organisation under an "o" claim, e.g.
{"id": "org_...", "rol": "admin", "slg": "..."}, with "v": 2 - NOT flat org_id /
org_role. Identity resolution reads both shapes and strips an "org:" role prefix.
The v2 token contains no email or name claim, so creator attribution is blank
unless custom claims are configured in the Clerk dashboard.

Multi-tenancy
The tenant is a Clerk Organization, mirrored locally as a Team. Every query on
Scan and ScanRiskAssessment is scoped by an EF Core global query filter. There
are no per-handler tenant checks anywhere; isolation is a property of the data
layer. An authenticated request with no active organisation is rejected with 403
BEFORE reaching any controller, because the filter's null-bypass exists for
background workers and must not be reachable from the web.

Identity synchronisation
Just-in-time on every request (covers the caller, always current, cannot fail to
be delivered, but can only ever see the caller; only non-null values overwrite
stored data) plus Clerk webhooks (cover everyone else, including members who
never opened the app, and removals; eventually consistent).

Clerk webhooks
POST /api/webhooks/clerk, anonymous, Svix-verified.
Verification: HMAC-SHA256 over "{svix-id}.{svix-timestamp}.{raw body}", compared
with a constant-time comparison, within a +/-5 minute window to defeat replay.
Invalid, tampered or stale requests are rejected with 401 before any handler
runs. With no signing secret configured the endpoint returns 503 rather than
accepting unverified events (fail closed).
Handled: user.created/updated/deleted, organization.created/updated/deleted,
organizationMembership.created/updated/deleted. Unknown types are acknowledged
with 200 so Svix does not retry them.
Two deliberate semantics: user.deleted revokes memberships but KEEPS the user row
(Scan.CreatedByUserId is DeleteBehavior.Restrict and authorship history should
outlive a membership change); organization.deleted deletes the Team, cascading
that tenant's scans and assessments.

Health and liveness
Each worker upserts a ServiceHeartbeats row every 10 seconds using
ON CONFLICT DO UPDATE. GET /api/health reports a service up only if its heartbeat
is within 30 seconds. Because the heartbeat is a database write, a green
indicator proves the worker is both running and able to reach the database.

Database tables
Scans (Id, RequestId unique, TeamId, CreatedByUserId, Target, Status, CreatedAt,
CompletedAt, ErrorMessage); ScanResults (ScanId, Port, Protocol, Service, State,
Product, Version); ScanRiskAssessments (Id, RequestId unique, TeamId,
CreatedByUserId, ScanId, Status, OverallRiskScore, RequestedAt, CompletedAt);
ScanRiskAssessmentFindings (port, service, product, version, Cpe, MatchedCves,
CvssScore, KevFlag, MatchConfidence); CveCacheEntries ((CpeUri, CveId) PK,
CvssScore, CachedAt); Teams (Id, ClerkOrgId unique, Name); Users (Id,
ClerkUserId unique, Email, Name); TeamMemberships (UserId, TeamId, Role);
OutboxMessages (Id, Type, Content, Status, timestamps); ServiceHeartbeats
(ServiceName PK, LastSeenAt).
Delete behaviour: TeamId cascades; CreatedByUserId is Restrict.
Status values live in Contracts.Constants.Status
(Pending/Running/Completed/Failed) and are used verbatim by the Go service.

API endpoints
GET  /api/health                                   anonymous
POST /api/scans                                    202
GET  /api/scans                                    paginated, page size 1-25
GET  /api/scans/{id}
GET  /api/scans/diff/{target}                      ?from=&to=
POST /api/scans/{scanId}/risk-assessments          202
GET  /api/scans/{scanId}/risk-assessments
GET  /api/scans/{scanId}/risk-assessment           latest
GET  /api/risk-assessments                         paginated
GET  /api/risk-assessments/{id}
GET  /api/dashboard/summary
GET  /api/dashboard/activity                       ?from=&to=&bucket=day|hour
POST /api/webhooks/clerk                           Svix signature

Scan diff
Compares two completed scans of one target and classifies every port into
addedPorts, removedPorts, changedPorts (with oldState and newState) and
unchangedPorts. With no parameters it compares the latest two; with "from" only
it compares that scan to the latest; with both it compares exactly those.
Validates that both scans belong to the target, are Completed, and are in
correct chronological order.

Dashboard
/api/dashboard/summary returns team-scoped totals computed in SQL: total scans,
total assessments, completed assessments, average overall risk score. Replaced an
earlier browser-side computation over a 25-item page that under-reported once a
team had more than 25 scans.
/api/dashboard/activity returns gap-filled, time-bucketed counts of scans and
assessments. Server-side bounds: 90 days maximum for day buckets, 3 days for hour
buckets. The UI offers 24h/7d/14d/30d ranges and toggleable series.

Portal routes
/ dashboard; /scans list; /scans/:id detail with risk-assessment panel;
/diff comparison; /status per-service liveness.
Server state is TanStack Query with adaptive polling: faster while any scan is
Pending or Running, stopping once terminal. Switching organisation clears the
query cache. Uses @clerk/react (Core 3), which renamed <SignedIn>/<SignedOut> to
<Show when="signed-in|signed-out"> and renamed appearance variables
(colorText -> colorForeground, colorInputBackground -> colorInput).

Testing
44 .NET unit tests (services, nmap XML parsing, 13 Svix signature verification
tests, 9 webhook handler tests). Go tests for the CPE resolver, the scorer, and
7 NVD client tests covering retry, replay, rate-limit spacing, API key
forwarding, non-retry on 4xx, exhaustion, and context cancellation. End-to-end
validation of the full stack. Philosophy: unit-test algorithms and business
rules, prove wiring with real end-to-end runs rather than mocking Kafka,
PostgreSQL and nmap.

EXPERIMENTAL EVALUATION - the only quantitative result
A hand-labelled corpus of 34 real "nmap -sV" banners drawn from Metasploitable2,
DVWA, common services, and a held-out set of products deliberately absent from
the dictionary. The resolver is compared against a naive baseline that lowercases
the product into both vendor and product and takes the version verbatim.
A sample counts as a true positive only if the emitted CPE matches the label
exactly AND clears the confidence threshold; banners with no catalogued product
must be abstained on. Reproduce with "go run ./cmd/cpe-eval -failures".

Results:
resolver     threshold  TP  FP  FN  TN  precision  recall  F1     accuracy
naive        0.00       11  22  0   1   0.333      1.000   0.500  0.353
dictionary   0.00       27  6   0   1   0.818      1.000   0.900  0.824
dictionary   0.50       23  0   8   3   1.000      0.742   0.852  0.765
dictionary   0.90       21  0   10  3   1.000      0.677   0.808  0.706

Interpretation:
- Dictionary plus version normalisation raises F1 from 0.500 to 0.900 at
  identical recall. The naive baseline collapses on distribution-patched versions
  and compound product strings.
- The confidence tier is what buys precision: rejecting below 0.5 eliminates
  every false positive, so the score is a usable abstention signal rather than
  decoration.
- Defects found by the harness, since fixed: MySQL was mapped to mysql:mysql when
  NVD publishes oracle:mysql, so MySQL CVE lookups silently returned nothing; and
  Tomcat's real banner "Apache Tomcat/Coyote JSP engine" fell back to
  apache:apache.
- Threats to validity: the corpus is small and hand-labelled, and the dictionary
  was corrected in response to it, so in-dictionary figures are optimistic; the
  held-out block is the genuine generalisation signal.
- Open calibration gap: four of the eight rejections at threshold 0.5 (OpenLDAP,
  HAProxy, Memcached, Dovecot) emitted the CORRECT CPE at 0.40 confidence. The
  fallback is right more often than it admits whenever vendor equals product.
  This was deliberately left unchanged to avoid tuning the algorithm to its own
  test set.

Known limitations (state these honestly)
- CVE data is fetched on demand and cached, not mirrored wholesale; first contact
  with a new product pays a network call.
- A distribution-backported fix cannot be distinguished from an unpatched
  upstream version by banner alone, so naive version matching can over-report.
  Surfaced as a confidence value rather than hidden.
- A finding with no product or version (nmap reports "tcpwrapped") yields no CPE,
  no CVEs and a score of 0. That 0 correctly means "nothing identifiable was
  found", NOT "this host is safe".
- Single PostgreSQL instance and single Kafka broker: an availability ceiling
  before a throughput one. Compute scales near-linearly by adding stateless
  worker replicas; the data layer is the real limit.
- In an egress-filtered network, external scan traffic is dropped and every port
  reads filtered, so external scans complete with empty results. A property of
  the network, not of Vantage.
- Not implemented: full offline NVD mirror; per-target risk-trend UI; user-
  selectable nmap profiles (deliberately withheld, since accepting arbitrary
  nmap flags from a web request is an injection surface); Kubernetes deployment
  (explicitly out of scope - do NOT discuss Kubernetes anywhere in the thesis).

Patterns to explain in chapter 4 and then reference in chapter 8
Clean Architecture; Ports and Adapters; microservices; event choreography;
polyglot services; shared database; light CQRS; transactional outbox; Strategy;
competing consumers; atomic claim with SKIP LOCKED; idempotency keys; async
request-reply with polling; cache-aside; client-side rate limiting with backoff
and jitter; graceful degradation; heartbeat health checks; backpressure;
two-tier timeout; webhook signature verification; JIT identity synchronisation;
repository; unit of work; result pattern; dependency injection with convention
registration; options pattern; DTO and mapper; validation orchestrator; factory;
middleware pipeline; global query filter; background services; expression-based
dynamic queries with an allow-list; anti-corruption layer; confidence scoring as
an abstention signal.

Patterns deliberately rejected (explain why in chapter 4)
Event sourcing; full CQRS with separate stores; saga/process manager;
database-per-service; API gateway/BFF; gRPC between services; distributed cache
such as Redis.

OUTPUT FORMAT
Markdown with numbered headings. Deliver the chapters in order. If output limits
prevent producing everything at once, stop at a chapter boundary, state clearly
where you stopped, and continue from exactly that point when asked, without
repeating earlier content and without summarising it.

Begin with Chapter 1.
```

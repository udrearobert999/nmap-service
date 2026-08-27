ROLE
You are an academic technical writer producing a Master's thesis in software
engineering for the Faculty of Automatics, Computers and Electronics, University
of Craiova.

LANGUAGE
Write the entire thesis in English.

TASK
Write the complete body of a Master's thesis about the software project
"Vantage", described in the PROJECT FACTS section below. Follow the CHAPTER PLAN
exactly, including its word budgets and its figure and table placeholders.

START AT CHAPTER 1 (INTRODUCTION). Do NOT generate the title page, originality
declaration, theme sheet, supervisor's report, acknowledgements, table of
contents, list of figures or list of tables. Those are written separately by the
author.

REFERENCE DOCUMENTS

Style and structure models. Use these ONLY as models for structure, tone, depth,
section numbering and academic register. Never copy their content, and never
write about their subject matter (augmented reality, web auditing, encrypted
messaging).
- @Cosmin-Gabriel Sulita - Master's Thesis.pdf — same faculty, written in
  English. Model the overall chapter progression on it.
- @alexandrescu_cristian_inginerie_software_2020.pdf — same faculty, same
  specialisation, closest domain. Imitate its IEEE-830 style requirements
  chapter and its heavy theory-to-implementation ratio.
- @Sistem de mesagerie cu criptare integrală.docx — imitate its per-component
  implementation depth and its enumerated use cases.

Authoritative source of truth for the system being described. Everything written
about Vantage must be consistent with these and with the PROJECT FACTS section
below.
- @01-HIGH-LEVEL-ARCHITECTURE.md — services, boundaries, data flow, security
  model, resilience, scaling limits. Use for chapters 1 and 7.
- @02-LOW-LEVEL-ARCHITECTURE.md — projects, layers, classes, database schema,
  outbox and claim mechanics, the Go pipeline, the portal, configuration. Use
  for chapter 8.
- @03-FUNCTIONALITIES.md — every feature and endpoint end to end, the CPE
  evaluation results, and everything not yet built. Use for chapters 7, 9 and 10.
- @04-PATTERNS-AND-TECHNOLOGIES.md — every pattern and library with its
  rationale, and the patterns deliberately rejected. Use for chapters 4 and 6.
- @CODE-EXAMPLES.md — short, real code listings from the codebase, from patterns
  through to business logic. When a code listing is needed, adapt one of these
  rather than inventing code. Keep listings between 5 and 20 lines and caption
  them "Listing X.Y". Use mainly in chapters 4 and 8.

LENGTH — THIS IS A HARD REQUIREMENT
Minimum 50 pages of body text. Target 70 to 80 pages, approximately 28,600
words. The per-chapter word budgets in the CHAPTER PLAN must be respected within
plus or minus 15 percent. Do not summarise and do not compress. If a section
feels short, develop it with definitions, worked explanations, comparisons and
trade-off analysis rather than padding with repetition.

Theory (chapters 2 to 6) must account for roughly 55 to 60 percent of the body.
This ratio matches the attached exemplars and is expected by the faculty.

STYLE RULES
- Formal academic register, third person, past tense for work performed. The
  only exceptions are section 1.2 (Motivation) and section 10.5 (Personal
  reflection), which are written in the first person, as in both exemplars.
- Define every technical term at first use.
- Every pattern in chapter 4 follows the same five-part shape: definition; the
  problem it solves; how it works; trade-offs and costs; alternatives that were
  considered and rejected.
- Explain WHY, not only WHAT. A design decision stated without its rationale and
  its cost is incomplete.
- Numbered headings to at most four levels, for example 8.6.4.
- Use footnotes for web sources, following the exemplars' footnote style.
- Code listings must be short and illustrative only, between 5 and 20 lines,
  each with a caption in the form "Listing X.Y - description". Never paste large
  files or entire classes.
- Prefer tables for comparisons and enumerations.

FORMATTING AND TYPOGRAPHY
Apply the following specification, which was measured from the attached example
theses. Do not invent a different typeface: Times New Roman is used throughout
both example theses and is the expected convention at this faculty.
- Body text: Times New Roman, 12 pt, 1.5 line spacing, justified, first-line
  indent 1.27 cm.
- Chapter heading (level 1): Times New Roman, 16 pt, bold, starting on a new
  page.
- Section heading (level 2): Times New Roman, 14 pt, bold.
- Subsection heading (level 3): Times New Roman, 13 pt, bold.
- Sub-subsection heading (level 4): Times New Roman, 12 pt, bold.
- Figure and table captions: Times New Roman, 10 pt, centred. Figure captions go
  below the figure, table captions above the table.
- Code listings: Courier New, 10 pt, single spacing, not justified.
- Footnotes: Times New Roman, 10 pt.
- Page margins: 2.5 cm top, bottom and right; 3 cm left for binding.
- Page numbers centred in the footer.

Headings MUST use Word's built-in Heading 1, Heading 2, Heading 3 and Heading 4
styles rather than manually formatted bold text, with the styles redefined to the
sizes above. This is essential: the author generates the table of contents, the
list of figures and the list of tables automatically from those styles, and
manually formatted headings would be invisible to that process.

Use automatic numbering for headings (1, 1.1, 1.1.1, 1.1.1.1), for figure
captions (Figure 1.1, Figure 1.2, …) and for table captions (Table 1.1, …),
numbered per chapter.

FIGURE AND TABLE PLACEHOLDERS
Insert every figure listed in the CHAPTER PLAN at the point in the text where it
is first referenced. Use exactly this three-line format, on its own lines:

    [FIGURE 4.4: Sequence of the transactional outbox pattern]
    (Description for the author: show the API writing the business row and the
    outbox row inside a single transaction, the Quartz job claiming pending rows
    every 2 seconds, publishing to Kafka, and marking them processed. Show the
    failure path in which publishing fails and the row remains pending for
    retry.)
    Figure 4.4 - Sequence of the transactional outbox pattern

That is: the bracketed placeholder naming the figure, then a parenthesised
description of what the image must show so the author can produce it, then the
final caption line.

Tables are the opposite: they must be FULLY POPULATED with real content taken
from PROJECT FACTS, never left as placeholders. For example:

    Table 9.3 - CPE resolution evaluation results
    | resolver | threshold | precision | recall | F1 |
    |---|---|---|---|---|
    | naive | 0.00 | 0.333 | 1.000 | 0.500 |
    | ... remaining rows from PROJECT FACTS ...

Always refer to figures and tables from the body text, for example "as shown in
Figure 4.4" or "Table 7.2 lists the functional requirements".

ACCURACY RULES — CRITICAL
- Use ONLY the facts in PROJECT FACTS and the attached project documents when
  writing about Vantage. Do not invent features, metrics, benchmarks, user
  numbers, performance figures or deployment details.
- The CPE evaluation results are the ONLY quantitative results about Vantage. Do
  not fabricate any other measurement.
- For general theory in chapters 2 to 6 you may draw on established public
  knowledge, but cite only real, verifiable sources. Never invent a citation,
  author, DOI or page number. If unsure of a source, describe the concept
  without attributing it.
- Where the project has limitations, state them honestly. The exemplars do this
  and it is expected of this thesis.
- Mark anything uncertain as [TO VERIFY] rather than guessing.
- Do not discuss Kubernetes anywhere in the thesis. It is explicitly out of
  scope.

================================================================================
CHAPTER PLAN
================================================================================

Summary of budgets:

| # | Chapter | Words | Pages |
|---|---|---|---|
| 1 | Introduction | 1,800 | 5 |
| 2 | Network scanning and service fingerprinting | 2,600 | 7 |
| 3 | Vulnerability intelligence: CVE, CPE, CVSS, KEV | 3,000 | 8 |
| 4 | Architectural and design patterns for distributed systems | 4,800 | 13 |
| 5 | Multi-tenancy, identity and application security | 2,700 | 7 |
| 6 | Technologies used | 2,200 | 6 |
| 7 | Analysis and design of the Vantage system | 3,600 | 9 |
| 8 | Implementation | 4,200 | 11 |
| 9 | Testing, validation and experimental evaluation | 2,600 | 7 |
| 10 | Conclusions | 1,100 | 3 |
| — | Bibliography and web references | — | 3 |
| | Total | ~28,600 | ~79 |

--------------------------------------------------------------------------------
CHAPTER 1 — INTRODUCTION (1,800 words)
--------------------------------------------------------------------------------
- 1.1 Purpose — 250
- 1.2 Motivation — 300, written in the first person
- 1.3 Context and problem statement — 300
- 1.4 Objectives of the project — 200
- 1.5 Development and design steps — 450, covering analysis, prototyping,
  design, implementation, testing and evaluation as distinct stages
- 1.6 Personal contributions — 150
- 1.7 Structure of the thesis — 150, one short paragraph per chapter

Requirements are NOT stated in this chapter. Chapter 1 sets out the problem and
the objectives; the requirements that follow from them are specified formally in
chapter 7. Section 1.4 may refer forward to chapter 7 but must not enumerate
requirements.

[FIGURE 1.1] Context diagram showing the operator, Vantage, the scanned
infrastructure, the external NVD and CISA KEV feeds, and Clerk.

--------------------------------------------------------------------------------
CHAPTER 2 — NETWORK SCANNING AND SERVICE FINGERPRINTING (2,600 words)
--------------------------------------------------------------------------------
2.1 The reconnaissance phase of a security assessment
2.2 TCP/IP fundamentals and port states: open, closed, filtered, unfiltered, and
    the tcpwrapped result
2.3 Scan techniques: SYN scan, connect scan, UDP scan, and skipping host
    discovery with -Pn
2.4 Service and version detection: the -sV flag, the probe database, banner
    grabbing, and why version detection is a precondition for any CVE matching
2.5 Timing templates, retry limits and host timeouts
2.6 Limitations: egress-filtered networks, distribution-backported versions,
    false negatives and false positives
2.7 Ethical and legal considerations of active scanning

[FIGURE 2.1] TCP three-way handshake compared with a SYN scan.
[FIGURE 2.2] Decision tree mapping probe responses to port states.
[FIGURE 2.3] Annotated example of nmap -sV output.
[TABLE 2.1] Comparison of scan techniques: speed, stealth, privilege required,
reliability.

--------------------------------------------------------------------------------
CHAPTER 3 — VULNERABILITY INTELLIGENCE (3,000 words)
--------------------------------------------------------------------------------
3.1 The vulnerability disclosure lifecycle
3.2 CVE identifiers and the CVE programme
3.3 The National Vulnerability Database and its 2.0 REST API
3.4 CPE: the 2.3 URI grammar, the official dictionary, and why the vendor is
    frequently not the product name
3.5 CVSS: versions 2, 3.0 and 3.1, base metric groups, vectors and severity
    bands
3.6 The CISA Known Exploited Vulnerabilities catalogue and exploit maturity as a
    risk signal distinct from severity
3.7 The matching problem: fuzzy banners, version normalisation, distribution
    patching, and the resulting false positives
3.8 Precision, recall, F1 and accuracy as evaluation measures for a matching
    system, including the confusion matrix and the precision/recall trade-off
3.9 Prior art in automated vulnerability correlation

[FIGURE 3.1] The vulnerability disclosure lifecycle.
[FIGURE 3.2] Anatomy of a CPE 2.3 URI with each component labelled.
[FIGURE 3.3] CVSS v3.1 base metric groups.
[FIGURE 3.4] The banner to CPE to CVE to score pipeline.
[TABLE 3.1] CVSS severity bands.
[TABLE 3.2] Confusion matrix with definitions of TP, FP, FN, TN, precision,
recall and F1.

--------------------------------------------------------------------------------
CHAPTER 4 — ARCHITECTURAL AND DESIGN PATTERNS (4,800 words)
--------------------------------------------------------------------------------
This is the theoretical core of the thesis. Each of the following sections must
follow the five-part shape: definition; the problem it solves; how it works;
trade-offs and costs; alternatives considered and rejected.

4.1  Monolithic versus microservice architectures
4.2  Clean Architecture and the dependency rule
4.3  Ports and Adapters (Hexagonal architecture)
4.4  Synchronous versus asynchronous integration
4.5  Message brokers, the distributed log abstraction, consumer groups, and
     delivery semantics (at-most-once, at-least-once, exactly-once)
4.6  The Transactional Outbox pattern and the dual-write problem
4.7  The Strategy pattern applied to dispatch by message type
4.8  Competing consumers and the atomic claim, including PostgreSQL
     FOR UPDATE SKIP LOCKED as a work-queue primitive
4.9  Idempotency and idempotency keys
4.10 Asynchronous request-reply with polling
4.11 Choreography versus orchestration
4.12 Repository and Unit of Work
4.13 The Result pattern as an alternative to exceptions for expected failures
4.14 Cache-aside
4.15 Client-side rate limiting, exponential backoff and jitter
4.16 Graceful degradation and containment of third-party failure
4.17 Health checks and heartbeats
4.18 Backpressure and bounded concurrency
4.19 The Anti-Corruption Layer
4.20 CQRS, and why only a partial read/write separation was adopted
4.21 Dependency injection, convention-based registration and the Options pattern

Also explain, with justification, the patterns that were deliberately rejected:
event sourcing; full CQRS with separate stores; saga or process manager;
database-per-service; API gateway or backend-for-frontend; gRPC between
services; a distributed cache such as Redis.

[FIGURE 4.1] Clean Architecture concentric rings with the dependency rule.
[FIGURE 4.2] Hexagonal architecture showing ports and adapters.
[FIGURE 4.3] The dual-write problem and its failure modes.
[FIGURE 4.4] Sequence of the transactional outbox pattern.
[FIGURE 4.5] Two workers claiming rows concurrently with SKIP LOCKED.
[FIGURE 4.6] Asynchronous request-reply with polling.
[FIGURE 4.7] Choreography compared with orchestration.
[FIGURE 4.8] Cache-aside read and populate flow.
[FIGURE 4.9] Retry timing with and without jitter.
[TABLE 4.1] Delivery semantics and their implications.
[TABLE 4.2] Summary of every pattern: pattern, problem solved, main trade-off.

--------------------------------------------------------------------------------
CHAPTER 5 — MULTI-TENANCY, IDENTITY AND APPLICATION SECURITY (2,700 words)
--------------------------------------------------------------------------------
5.1 Tenancy models: silo, bridge and pool
5.2 Isolation strategies and the forgotten-WHERE-clause failure mode
5.3 Row-level isolation enforced by query filters at the data-access layer
5.4 OAuth 2.0, OpenID Connect, and the structure and validation of JSON Web
    Tokens
5.5 Externalised identity providers and the organisation model
5.6 Session token claim shapes and the consequences of provider versioning
5.7 Identity synchronisation strategies: just-in-time versus webhooks, and why
    neither is sufficient alone
5.8 Webhook security: HMAC signatures, replay windows, constant-time comparison
    and timing attacks
5.9 Secrets management, injection surfaces and least privilege

[FIGURE 5.1] Silo, bridge and pool tenancy models.
[FIGURE 5.2] Request flow through a global query filter.
[FIGURE 5.3] Structure of a signed JWT.
[FIGURE 5.4] Webhook signature verification sequence including rejection paths.
[TABLE 5.1] Tenancy models compared: isolation, cost, operational complexity.

--------------------------------------------------------------------------------
CHAPTER 6 — TECHNOLOGIES USED (2,200 words)
--------------------------------------------------------------------------------
6.1  .NET 10 and C#
6.2  Go
6.3  PostgreSQL and the specific primitives relied upon
6.4  Entity Framework Core and Npgsql
6.5  Apache Kafka in KRaft mode
6.6  MassTransit and the Confluent.Kafka client
6.7  Quartz.NET
6.8  nmap
6.9  React, Vite, TypeScript, Tailwind CSS, shadcn/ui, TanStack Query, recharts
6.10 Clerk
6.11 Docker and Docker Compose
6.12 Justification of each choice against its realistic alternatives

[FIGURE 6.1] The technology stack arranged by layer.
[TABLE 6.1] Technology, role in the system, alternatives considered, rationale.

--------------------------------------------------------------------------------
CHAPTER 7 — ANALYSIS AND DESIGN OF THE VANTAGE SYSTEM (3,600 words)
--------------------------------------------------------------------------------
Follows the IEEE-830 shape used by Alexandrescu. THIS CHAPTER IS WHERE THE
FUNCTIONAL AND NON-FUNCTIONAL REQUIREMENTS ARE SPECIFIED.

- 7.1 Introduction — 350
  - 7.1.1 Motivation
  - 7.1.2 Scope
  - 7.1.3 Definitions, acronyms and abbreviations
- 7.2 General description — 700
  - 7.2.1 Product perspective
  - 7.2.2 Product functions
  - 7.2.3 User characteristics
  - 7.2.4 Constraints
  - 7.2.5 Assumptions and dependencies
- 7.3 Specific requirements — 1,300
  - 7.3.1 External interfaces — 350: user, hardware, software, communication
  - 7.3.2 Functional requirements — 600. Numbered RF-01 onwards, grouped by the
    three user classes: unauthenticated user, authenticated team member, and
    team administrator. Each requirement states an identifier, a description, a
    priority (mandatory or optional), and the use case or API endpoint that
    satisfies it. Cover at minimum: sign-in and organisation selection; queueing
    a scan; listing and filtering scans; viewing a scan and its results;
    comparing two scans of a target; requesting a risk assessment; viewing
    findings with CVSS, KEV flag and match confidence; viewing dashboard totals
    and activity over a time range; viewing service status; and roster
    synchronisation from the identity provider.
  - 7.3.3 Non-functional requirements — 350. Numbered RNF-01 onwards, grouped by
    category: performance and responsiveness; scalability; reliability and
    availability; security; maintainability; portability; usability. Each must
    be phrased so that it is verifiable rather than aspirational.
- 7.4 System architecture — 500
- 7.5 Data model — 400
- 7.6 Use cases — 350, enumerated, each with actor, precondition, main flow,
  alternative flows and postcondition

Requirements defined here must be traceable: chapter 8 states which component
implements each, and chapter 9 states how each was verified.

[FIGURE 7.1] High-level architecture of the four services and shared
infrastructure.
[FIGURE 7.2] Deployment diagram of the Docker Compose topology.
[FIGURE 7.3] Use-case diagram.
[FIGURE 7.4] Entity-relationship diagram of the database.
[FIGURE 7.5] Sequence diagram of the scan pipeline.
[FIGURE 7.6] Sequence diagram of the risk-assessment pipeline.
[FIGURE 7.7] State machine of a scan.
[TABLE 7.1] Definitions, acronyms and abbreviations.
[TABLE 7.2] Functional requirements, RF-01 onwards.
[TABLE 7.3] Non-functional requirements, RNF-01 onwards.
[TABLE 7.4] Use-case index.
[TABLE 7.5] Database entities and their purpose.

--------------------------------------------------------------------------------
CHAPTER 8 — IMPLEMENTATION (4,200 words)
--------------------------------------------------------------------------------
8.1  Solution structure and the mapping of projects onto architectural layers
8.2  The REST API: controllers, the middleware pipeline, Result handling and
     output caching
8.3  Persistence: the DbContext, entity configurations, migrations,
     repositories, the Unit of Work, and the global query filter
8.4  The outbox and its type-dispatched publishers
8.5  The scan worker: the Kafka consumer, the nmap runner, argument
     construction, the two-tier timeout, and the XML parser
8.6  The vulnerability-intelligence service
     - 8.6.1 Package structure and ports
     - 8.6.2 The CPE resolver
     - 8.6.3 CVE lookup and the local cache
     - 8.6.4 The NVD client: rate limiting, retry and backoff
     - 8.6.5 The KEV catalogue
     - 8.6.6 The scorer
8.7  Authentication and identity: auth modes, claim resolution, just-in-time
     synchronisation
8.8  Clerk webhooks: signature verification, event handlers, and the deletion
     semantics
8.9  Dashboard aggregation endpoints
8.10 The portal
8.11 Orchestration with Docker Compose

Each section must state which requirements from chapter 7 it satisfies.

[FIGURE 8.1] Project dependency graph of the .NET solution.
[FIGURE 8.2] The ASP.NET Core middleware pipeline as configured.
[FIGURE 8.3] Class diagram of the outbox publishers and the Strategy dispatch.
[FIGURE 8.4] Timeline of the two-tier nmap timeout showing both outcomes.
[FIGURE 8.5] Go package and port diagram.
[FIGURE 8.6] Flowchart of CPE resolution including the fallback path.
[FIGURE 8.7] The NVD rate limiter and retry state machine.
[FIGURE 8.8] Portal screenshot: dashboard with summary cards and activity chart.
[FIGURE 8.9] Portal screenshot: scans list.
[FIGURE 8.10] Portal screenshot: scan detail with the findings table.
[FIGURE 8.11] Portal screenshot: scan diff page.
[FIGURE 8.12] Portal screenshot: system status page.
[FIGURE 8.13] Portal screenshot: sign-in and organisation selection.
[FIGURE 8.14] Docker Compose container topology.
[TABLE 8.1] API endpoints with method, path, authentication and purpose.
[TABLE 8.2] Configuration variables and their meaning.
[TABLE 8.3] CPE confidence tiers.

--------------------------------------------------------------------------------
CHAPTER 9 — TESTING, VALIDATION AND EXPERIMENTAL EVALUATION (2,600 words)
--------------------------------------------------------------------------------
9.1 Testing strategy and its rationale
9.2 Unit tests across the .NET and Go codebases
9.3 Security-focused tests: signature verification, replay rejection, tampering
9.4 End-to-end validation of the full stack
9.5 Experimental evaluation of the CPE resolver
    - 9.5.1 Corpus construction and labelling
    - 9.5.2 The baseline
    - 9.5.3 Metric definitions and the acceptance rule
    - 9.5.4 Results
    - 9.5.5 Analysis and interpretation
    - 9.5.6 Defects discovered by the harness
    - 9.5.7 Threats to validity
9.6 Observations on behaviour under degraded conditions

[FIGURE 9.1] The test pyramid as applied to this project.
[FIGURE 9.2] Precision and recall plotted against the confidence threshold.
[FIGURE 9.3] Bar chart comparing baseline and dictionary F1.
[TABLE 9.1] Test suites, their scope and their counts.
[TABLE 9.2] Composition of the evaluation corpus by source.
[TABLE 9.3] CPE resolution evaluation results.
[TABLE 9.4] Defects discovered by the evaluation harness.

--------------------------------------------------------------------------------
CHAPTER 10 — CONCLUSIONS (1,100 words)
--------------------------------------------------------------------------------
10.1 Results achieved
10.2 Fulfilment of the objectives stated in chapter 1
10.3 Limitations
10.4 Future work
10.5 Personal reflection, written in the first person

[TABLE 10.1] Objectives from chapter 1 mapped to outcomes.

================================================================================
PROJECT FACTS — AUTHORITATIVE
================================================================================

NAME AND PURPOSE
Vantage is a multi-tenant network security platform. It answers two questions
about an organisation's infrastructure: what is exposed, and how dangerous it is.
It discovers reachable services by scanning, then correlates them against public
vulnerability intelligence to produce scored findings.

CENTRAL THESIS
Decoupling network scanning from vulnerability correlation into independent,
event-driven services. The two workloads have different characteristics:
scanning is I/O-bound, slow and network-facing; correlation is data-bound and
depends on external feeds that evolve independently. Coupling them means the
slowest and least reliable part governs the behaviour of the whole.

SERVICES (FOUR)
1. REST API — .NET 10 and C#. Owns lifecycle state, authentication, tenancy and
   validation, and publishes work. Performs no slow work itself. Port 8080.
2. Scan worker — .NET 10. Consumes scan requests, executes nmap, parses the XML
   output, writes results.
3. vulnintel-service — Go. Consumes assessment requests, resolves CPEs, looks up
   CVEs, flags KEV entries, scores findings. Port 8090.
4. Portal — React 18, Vite 5 and TypeScript single-page application. Port 5173.

Supporting infrastructure: PostgreSQL as a single shared database with one EF
Core AppDbContext and 15 migrations; Apache Kafka in KRaft mode with a single
broker; Kafka UI for inspection; Clerk as the external identity provider. Eight
containers orchestrated by a single Docker Compose file.

WHY POLYGLOT
Writing the vulnerability service in Go is a deliberate forcing function for
boundary integrity. Services sharing a language drift into shared helpers and
leaked domain types until they are one deployable in disguise. A different
runtime makes that structurally impossible: the Go service cannot import a C#
class, so the only coupling is a Kafka JSON message contract and a database
table shape. Go also suits a lean, concurrent network and data service with a
small memory footprint and fast start-up. The cost is two toolchains, two
dependency sets and duplicated DTO definitions.

SHARED DATABASE TRADE-OFF
One database rather than database-per-service. Benefits: read-your-writes
consistency for the polling user experience; structural symmetry, since the Go
worker writes results exactly as the .NET worker does; and no need for a
result-return topic, a consumer in the API and reconciliation logic. Cost: the
Go service is coupled to an EF-managed schema, and a breaking migration must be
coordinated across two codebases. The boundary remains real because the services
share no code and no runtime.

ASYNCHRONOUS FLOW (IDENTICAL FOR BOTH PIPELINES)
A POST request causes the API to write the business row AND an outbox row in a
single transaction, then return 202 Accepted with a Location header and the new
identifier. A Quartz job running every 2 seconds claims pending outbox rows and
publishes them to Kafka. The relevant worker consumes the message, atomically
claims the work by moving it from Pending to Running, performs the slow
operation, writes results and marks it Completed. The client polls a GET endpoint
for status. The symmetry between the two pipelines is deliberate: the
risk-assessment pipeline is a one-for-one structural clone of the scan pipeline.

TRANSACTIONAL OUTBOX
Solves the dual-write problem, in which writing a business row to PostgreSQL and
publishing to Kafka are two systems with no shared transaction; if one succeeds
and the other fails, state diverges. The claim SQL is of the form
UPDATE "OutboxMessages" SET "Status" = 'Processing' WHERE "Id" IN (SELECT "Id"
FROM "OutboxMessages" WHERE "Status" = 'Pending' ORDER BY "OccurredOnUtc" FOR
UPDATE SKIP LOCKED LIMIT n) RETURNING *. SKIP LOCKED allows concurrent claimers
to skip rows another claimer has locked rather than blocking on them. Delivery is
at-least-once, which is precisely why every consumer is idempotent.

DISPATCH BY TYPE (STRATEGY PATTERN)
OutboxMessage.Type selects an IOutboxMessagePublisher implementation:
- ScanRequestMessage, published through MassTransit to scan-requests-topic
- ScanRiskAssessmentRequestedMessage, published through the raw Confluent.Kafka
  producer to scan-risk-assessment-requests-topic
Two transports are required because MassTransit wraps payloads in its own
envelope, which the Go consumer, reading plain JSON, cannot parse. This is a
genuine polyglot interoperability constraint, discovered by reading MassTransit's
source rather than its documentation. The Strategy pattern is what made
accommodating it a new class rather than a rewrite.

ATOMIC CLAIM FOR SCANS
An ExecuteUpdate with WHERE Id = id AND Status = 'Pending' setting Status to
'Running'; only the caller whose rows-affected count is greater than zero
proceeds. This prevents double processing when Kafka redelivers a message. A
read-then-write approach would leave a window between the SELECT and the UPDATE
in which another worker could claim the same row.

IDEMPOTENCY
An X-Idempotency-Key header is stored as a unique RequestId. The create path both
checks for an existing row and catches the unique-constraint violation, so two
genuinely simultaneous identical requests collapse to one scan.

BACKPRESSURE
The scan consumer sets PrefetchCount to twice the message limit, together with
ConcurrentMessageLimit, ConcurrentConsumerLimit and a 5-second checkpoint
interval, so a burst of requests cannot pull unbounded work into one worker.

NMAP INVOCATION
nmap -Pn -sV -T4 --max-retries 1 --host-timeout 600s -oX - <target>
-Pn skips host discovery; -sV performs service and version detection and is
required because a CPE needs product and version; -T4 is a faster timing
template; --max-retries 1 stops re-probing dropped ports; -oX - writes XML to
standard output.

Two-tier timeout, deliberately unequal:
- --host-timeout 600s: nmap self-limits, exits with code 0 and partial XML, and
  the scan is recorded as Completed.
- Nmap:TimeoutSeconds = 660s: a hard Process.Kill(entireProcessTree) whose
  output is discarded, and the scan is recorded as Failed.
The graceful budget must fire first so partial results survive; the process kill
exists only as a backstop for nmap itself hanging. Setting them equal would
create a race in which the kill destroys results nmap was about to emit.
Failure is judged by exit code and empty output, not by stderr being non-empty,
because nmap writes non-fatal warnings, including its own host-timeout notice, to
stderr.

CPE RESOLUTION (THE ANTI-CORRUPTION LAYER)
Resolve(product, version) returns a URI and a confidence value.
1. Lowercase and trim the product, then look it up in a curated vendor:product
   dictionary. Examples: openssh maps to openbsd:openssh; apache httpd maps to
   apache:http_server; mysql maps to oracle:mysql; postgresql maps to
   postgresql:postgresql; apache tomcat/coyote jsp engine maps to apache:tomcat.
   The dictionary exists because the vendor is frequently not the product name,
   and getting it wrong means CVE lookups silently return nothing.
2. Normalise the version with the regular expression
   [0-9]+(\.[0-9]+)*([a-z][0-9a-z]*)?, which turns "6.6.1p1 Ubuntu 2ubuntu2.13"
   into "6.6.1p1" and "8.3.0 - 8.3.7" into "8.3.0".
3. Emit cpe:2.3:a:{vendor}:{product}:{version}:*:*:*:*:*:*:*
4. If the product is unknown, fall back to using its first token as both vendor
   and product, and say so through a low confidence value.

Confidence tiers: dictionary hit with version 0.9; dictionary hit without version
0.6; fallback with version 0.4; fallback without version 0.2.

CVE LOOKUP (CACHE-ASIDE)
Check the CveCacheEntries table, which has a 7-day time-to-live. On a miss, call
the NVD 2.0 REST API and populate the cache; a repeat assessment of the same
product therefore makes no network call at all. CVSS is taken from v3.1, falling
back to v3.0, then to v2. If NVD is unreachable, the lookup logs a warning and
returns whatever is available, so the assessment still completes rather than
failing outright.

NVD CLIENT HARDENING
A monotonic next-slot rate limiter spaces requests at approximately one per 6.5
seconds anonymously, or one per 0.7 seconds when NVD_API_KEY is configured, so
concurrent assessments queue rather than burst. Failures retry up to four
attempts with exponential backoff plus jitter, honouring a Retry-After header
when present. Retries apply to 429, 403, 5xx and transport errors; a 4xx fails
immediately rather than consuming quota. Every wait is context-aware, so a
cancelled assessment stops sleeping immediately.

SCORING
finding_score = min(base * kev_multiplier, 10)
- base is the CVE's CVSS score. If a service has no matched CVEs, a heuristic by
  service name is used instead: telnet 7.0; rdp and ms-wbt-server 6.0; vnc 6.0;
  ftp and ftp-data 5.0; smb, microsoft-ds and netbios-ssn 5.0; otherwise 0.0.
  This exists so a dangerous but unversioned service is not scored as harmless.
- kev_multiplier is 1.15 when a matched CVE appears in the CISA KEV catalogue,
  because a known-exploited vulnerability deserves more weight than severity
  alone.
overall = min(0.7 * worst + 0.3 * average, 10)
The 70/30 weighting is a judgement: a single critical exposure should dominate
the headline number, but breadth should still move it. A pure maximum would
ignore how many services are affected; a pure mean would let many benign services
bury one critical finding.

AUTHENTICATION
Auth:Mode selects the scheme independently of ASPNETCORE_ENVIRONMENT:
- DevHeaders trusts X-Dev-Subject, X-Dev-Org, X-Dev-Role and X-Dev-Email, and
  refuses to start outside the Development environment.
- Clerk validates real Clerk JWTs against Clerk:Authority, with an optional
  Clerk:Audience, and fails fast at start-up if the authority is missing.
Decoupling these from the environment was necessary because previously enabling
real JWT validation also disabled Swagger and forced HTTPS redirection, which is
why real token validation had never been exercised.

CLERK V2 SESSION TOKEN
The token nests the active organisation under an "o" claim, for example
{"id": "org_...", "rol": "admin", "slg": "..."} with "v": 2. There is no flat
org_id or org_role claim. Identity resolution reads both shapes and strips an
"org:" prefix from the role. The v2 token contains no email or name claim, so
creator attribution is blank unless custom claims are configured in the identity
provider's dashboard.

MULTI-TENANCY
The tenant is a Clerk Organization, mirrored locally as a Team. Every query on
Scan and ScanRiskAssessment is scoped by an EF Core global query filter. There
are no per-handler tenant checks anywhere in the codebase; isolation is a
property of the data-access layer, so a newly added endpoint is isolated by
default rather than by the developer remembering. An authenticated request with
no active organisation is rejected with 403 before it reaches any controller,
because the filter's null-bypass exists for background workers and must never be
reachable from the web.

IDENTITY SYNCHRONISATION
Just-in-time synchronisation runs on every request and covers the caller. It is
always current and cannot fail to be delivered, but it can only ever see the
caller. Only non-null values overwrite stored data, so a token missing a claim
never blanks existing information. Webhooks cover everyone else, including
members who have never opened the application, and propagate removals, but are
eventually consistent. Neither mechanism is sufficient alone.

CLERK WEBHOOKS
POST /api/webhooks/clerk, anonymous, verified as a Svix signature.
Verification computes HMAC-SHA256 over "{svix-id}.{svix-timestamp}.{raw body}",
compares it in constant time, and enforces a plus or minus 5-minute timestamp
window to defeat replay. Invalid, tampered and stale requests are rejected with
401 before any handler runs. With no signing secret configured the endpoint
returns 503 rather than accepting unverified events, so it fails closed.
Handled events: user.created, user.updated, user.deleted, organization.created,
organization.updated, organization.deleted, organizationMembership.created,
organizationMembership.updated, organizationMembership.deleted. Unknown types are
acknowledged with 200 so the delivery service does not retry them.
Two deliberate semantics: user.deleted revokes the user's memberships but keeps
the User row, because Scan.CreatedByUserId uses DeleteBehavior.Restrict and
authorship history should outlive a membership change; organization.deleted
deletes the Team, cascading that tenant's scans and assessments, because the
tenant no longer exists and the rows would otherwise be unreachable behind the
query filter.

HEALTH AND LIVENESS
Each worker upserts a row into ServiceHeartbeats every 10 seconds using ON
CONFLICT DO UPDATE. GET /api/health reports a service as up only if its heartbeat
is within 30 seconds. Because the heartbeat is a database write, a green
indicator proves the worker is both running and able to reach the database, which
an HTTP ping could not establish.

DATABASE TABLES
Scans (Id, RequestId unique, TeamId, CreatedByUserId, Target, Status, CreatedAt,
CompletedAt, ErrorMessage)
ScanResults (ScanId, Port, Protocol, Service, State, Product, Version)
ScanRiskAssessments (Id, RequestId unique, TeamId, CreatedByUserId, ScanId,
Status, OverallRiskScore, RequestedAt, CompletedAt)
ScanRiskAssessmentFindings (port, service, product, version, Cpe, MatchedCves,
CvssScore, KevFlag, MatchConfidence)
CveCacheEntries (CpeUri and CveId composite primary key, CvssScore, CachedAt)
Teams (Id, ClerkOrgId unique, Name)
Users (Id, ClerkUserId unique, Email, Name)
TeamMemberships (UserId, TeamId, Role)
OutboxMessages (Id, Type, Content, Status, timestamps)
ServiceHeartbeats (ServiceName primary key, LastSeenAt)
Delete behaviour: TeamId cascades, so deleting a team removes its data;
CreatedByUserId is Restrict, so a user who authored scans cannot be deleted.
Status values live in Contracts.Constants.Status as Pending, Running, Completed
and Failed, and are used verbatim by the Go service, forming one of the two
contracts crossing the language boundary.

API ENDPOINTS
GET  /api/health                                 anonymous
POST /api/scans                                  returns 202
GET  /api/scans                                  paginated, page size 1 to 25
GET  /api/scans/{id}
GET  /api/scans/diff/{target}                    optional from and to parameters
POST /api/scans/{scanId}/risk-assessments        returns 202
GET  /api/scans/{scanId}/risk-assessments
GET  /api/scans/{scanId}/risk-assessment         latest for that scan
GET  /api/risk-assessments                       paginated
GET  /api/risk-assessments/{id}
GET  /api/dashboard/summary
GET  /api/dashboard/activity                     from, to, bucket=day|hour
POST /api/webhooks/clerk                         Svix signature required
Status codes used: 202 for accepted work, 200 for reads, 400 for validation
failures with the specific message, 401 unauthenticated, 403 no active
organisation, 404 unknown identifier, 503 webhook endpoint unconfigured.

SCAN TARGET VALIDATION
Accepts an IPv4 address, an IPv6 address, or a hostname. Rejection returns 400
with the specific reason, for example "Target must be a valid IP address or
hostname." Single-label names such as "postgres" are rejected because a hostname
must be dotted.

SCAN DIFF
Compares two completed scans of one target and classifies every port into
addedPorts, removedPorts, changedPorts with an oldState and a newState, and
unchangedPorts. With no parameters it compares the latest two completed scans;
with only "from" it compares that scan against the latest; with both it compares
exactly those two. Before comparing, the API validates that both scans belong to
the target, are Completed, and are in the correct chronological order.

DASHBOARD
/api/dashboard/summary returns team-scoped totals computed in SQL: total scans,
total assessments, completed assessments, and the average overall risk score
across completed assessments, rounded to one decimal. This replaced an earlier
browser-side computation over a 25-item page that silently under-reported once a
team had more than 25 scans.
/api/dashboard/activity returns gap-filled, time-bucketed counts of scans and
assessments, so empty buckets appear as zero rather than vanishing. Server-side
bounds cap a day-bucketed range at 90 days and an hour-bucketed range at 3 days,
so a hand-crafted query cannot request an unbounded table scan. The interface
offers 24h, 7d, 14d and 30d ranges, with 24h switching to hourly buckets, and a
clickable legend to toggle each series.

PORTAL
Routes: / for the dashboard; /scans for the list; /scans/:id for detail with the
risk-assessment panel; /diff for comparison; /status for per-service liveness.
Server state is managed entirely by TanStack Query with adaptive polling: faster
while any scan is Pending or Running, and stopping once everything is terminal.
Switching organisation clears the query cache so no other tenant's data survives
the switch. The portal uses @clerk/react (Core 3), which renamed <SignedIn> and
<SignedOut> to <Show when="signed-in"> and <Show when="signed-out">, and renamed
the appearance variables, for example colorText became colorForeground and
colorInputBackground became colorInput. Error handling prefers the API's specific
validation messages over a generic failure label.

TESTING
44 .NET unit tests covering the service layer, nmap XML parsing, 13 Svix
signature verification tests and 9 webhook handler tests. Go tests cover the CPE
resolver, the scorer, and the NVD client with 7 tests covering retry after a
rate-limit response, non-retry on 4xx, exhaustion after the maximum attempts,
API key forwarding, request spacing by the rate limiter, context cancellation,
and result ordering. End-to-end validation exercises the full stack. The
philosophy is to unit-test algorithms and business rules, and to prove the wiring
with real end-to-end runs rather than mocking Kafka, PostgreSQL and nmap.

EXPERIMENTAL EVALUATION — THE ONLY QUANTITATIVE RESULT
A hand-labelled corpus of 34 real "nmap -sV" banners drawn from Metasploitable2,
DVWA, common production services, and a held-out set of products deliberately
absent from the dictionary. The resolver is compared against a naive baseline
that lowercases the product into both the vendor and product fields and takes the
version string verbatim. A sample counts as a true positive only if the emitted
CPE matches the label exactly AND clears the confidence threshold; banners with
no catalogued product must be abstained on. The experiment is reproducible with
"go run ./cmd/cpe-eval -failures".

Results:
resolver     threshold  TP  FP  FN  TN  precision  recall  F1     accuracy
naive        0.00       11  22  0   1   0.333      1.000   0.500  0.353
dictionary   0.00       27  6   0   1   0.818      1.000   0.900  0.824
dictionary   0.50       23  0   8   3   1.000      0.742   0.852  0.765
dictionary   0.90       21  0   10  3   1.000      0.677   0.808  0.706

Interpretation:
- The dictionary combined with version normalisation raises F1 from 0.500 to
  0.900 at identical recall. The naive baseline's precision collapses because
  distribution-patched versions and compound product strings never match.
- The confidence tier is what buys precision: rejecting matches below 0.5
  eliminates every false positive, so the confidence score is a usable
  abstention signal rather than decoration.
- Defects found by the harness, since fixed: MySQL was mapped to mysql:mysql when
  NVD publishes oracle:mysql, so MySQL CVE lookups silently returned nothing; and
  Tomcat's real banner "Apache Tomcat/Coyote JSP engine" fell back to
  apache:apache.
- Threats to validity: the corpus is small and hand-labelled, and the dictionary
  was corrected in response to it, so the in-dictionary figures are optimistic.
  The held-out block is the genuine generalisation signal.
- Open calibration gap: four of the eight rejections at threshold 0.5, namely
  OpenLDAP, HAProxy, Memcached and Dovecot, emitted the CORRECT CPE at 0.40
  confidence. The fallback is right more often than its confidence admits
  whenever the vendor equals the product. This was deliberately left unchanged to
  avoid tuning the algorithm to its own test set.

KNOWN LIMITATIONS — STATE THESE HONESTLY
- CVE data is fetched on demand and cached, not mirrored wholesale, so first
  contact with a new product pays a network call.
- A distribution-backported fix cannot be distinguished from an unpatched
  upstream version by banner alone, so naive version matching can over-report.
  This is surfaced as a confidence value rather than hidden.
- A finding with no product or version, which nmap reports as tcpwrapped, yields
  no CPE, no CVEs and a score of 0. That 0 correctly means "nothing identifiable
  was found", not "this host is safe". Absence of evidence is reported as
  absence, not as safety.
- A single PostgreSQL instance and a single Kafka broker constitute an
  availability ceiling before a throughput one. Compute scales near-linearly by
  adding stateless worker replicas up to the topic partition count; the data
  layer is the real limit.
- In an egress-filtered network, external scan traffic is dropped and every port
  reads as filtered, so external scans complete with empty results. This is a
  property of the network, not of the system.
- Not implemented: a full offline NVD mirror; a per-target risk-trend interface;
  user-selectable nmap profiles, deliberately withheld because accepting
  arbitrary nmap flags from a web request is an injection surface, so any future
  exposure should be an allow-list of named profiles rather than raw flags.

PATTERNS TO EXPLAIN IN CHAPTER 4 AND THEN REFERENCE IN CHAPTER 8
Clean Architecture; Ports and Adapters; microservices; event choreography;
polyglot services; shared database; light CQRS; transactional outbox; Strategy;
competing consumers; atomic claim with SKIP LOCKED; idempotency keys;
asynchronous request-reply with polling; cache-aside; client-side rate limiting
with backoff and jitter; graceful degradation; heartbeat health checks;
backpressure; the two-tier timeout; webhook signature verification;
just-in-time identity synchronisation; repository; unit of work; the Result
pattern; dependency injection with convention-based registration; the Options
pattern; DTO and mapper; validation orchestrator; factory; middleware pipeline;
global query filter; background services; expression-based dynamic queries with
an allow-list; anti-corruption layer; and confidence scoring as an abstention
signal.

PATTERNS DELIBERATELY REJECTED — EXPLAIN WHY IN CHAPTER 4
Event sourcing, because current state is what the interface needs and a full
event log would add replay and projection machinery for an audit benefit already
met by status timestamps and attribution. Full CQRS with separate stores, because
read and write volumes are comparable and both small. Saga or process manager,
because no multi-step distributed transaction exists to coordinate or compensate.
Database-per-service, a documented trade-off in favour of consistency and
symmetry. API gateway or backend-for-frontend, because there is one
single-page application and one API. gRPC between services, because the services
communicate by events rather than calls, and a synchronous RPC channel would
reintroduce exactly the coupling the architecture removes. A distributed cache
such as Redis, because the CVE cache is durable shared data that belongs in
PostgreSQL and output caching is per-instance and cheap to rebuild.

================================================================================
OUTPUT FORMAT
================================================================================
Produce a downloadable Microsoft Word document (.docx), formatted exactly as
specified in the FORMATTING AND TYPOGRAPHY section: Times New Roman throughout,
Word's built-in Heading 1 to Heading 4 styles redefined to the sizes given,
automatic heading and caption numbering, 1.5 line spacing, justified body text,
and the stated page margins.

The document contains the body only, starting at Chapter 1. It must NOT contain
a title page, originality declaration, theme sheet, supervisor's report,
acknowledgements, table of contents, list of figures or list of tables. The
author writes those separately and generates the contents lists from the heading
styles, which is why those styles must be used properly.

Figure placeholders remain as text in the document, in the three-line format
given above, so the author can replace each one with the finished image.

If a .docx cannot be produced, fall back to Markdown using # for Chapter, ## for
section, ### for subsection and #### for sub-subsection, applied strictly and
consistently so the heading levels map onto Word styles when the document is
assembled, and state clearly that a fallback was used.

Deliver the chapters in order. If output limits prevent producing everything in
one response, stop at a chapter boundary, state clearly where you stopped, and
continue from exactly that point when asked, without repeating earlier content
and without summarising it. If the document is delivered in several parts, the
final response must assemble all chapters into a single .docx.

Begin with Chapter 1.

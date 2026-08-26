# Vantage — Functionalities Explained

Every capability the system has today, how it behaves, what it guarantees, and
where its edges are. The final section covers what is designed but not yet
built.

---

## 1. Feature map

| Area | Capability | Status |
|---|---|---|
| Scanning | Queue a scan, poll to completion, view results | Done |
| Scanning | Compare two scans of a target (diff) | Done |
| Risk | Request an assessment, poll, view scored findings | Done |
| Risk | CPE resolution with confidence, CVE lookup, KEV flagging | Done |
| Dashboard | Exact totals + average risk | Done |
| Dashboard | Time-ranged activity chart, two toggleable series | Done |
| Ops | System status page with per-service liveness | Done |
| Auth | Clerk sign-in, organisations, JWT validation, dev-header mode | Done |
| Auth | Roster sync via JIT + webhooks | Done |
| Quality | CPE precision/recall evaluation harness | Done |
| Risk | Per-target risk trend over time | Not built |
| Data | Full offline NVD mirror | Not built |

---

## 2. Scanning

### 2.1 Creating a scan

`POST /api/scans` with `{"target": "..."}` and an `X-Idempotency-Key` header.

**Target validation** accepts an IPv4 address, IPv6 address, or hostname.
Rejection returns **400** with the specific reason
(*"Target must be a valid IP address or hostname."*), which the portal surfaces
verbatim in a toast.

> Single-label names such as `postgres` are rejected — a hostname must be
> dotted. Use the container's IP for internal targets.

**What happens on success:** the API writes the `Scan` row **and** an
`OutboxMessage` in one transaction and returns **202 Accepted** with a
`Location` header and the new id. It does not wait for the scan.

**Idempotency.** `X-Idempotency-Key` is stored as a unique `RequestId`. Retrying
with the same key returns the original scan instead of creating a second one.
The create path both checks first *and* catches the unique-constraint violation,
so two genuinely simultaneous requests still collapse to one scan.

### 2.2 Lifecycle

```
Pending ──claim──▶ Running ──┬──▶ Completed   (nmap finished, results stored)
                             └──▶ Failed      (timeout, unresolvable target, nmap error)
```

Only one worker can move a scan out of `Pending` — the claim is a conditional
`UPDATE` whose row count decides the winner — so a redelivered Kafka message
cannot cause a double scan.

`Failed` scans carry a human-readable `errorMessage`, for example
*"The Nmap scan for 'scanme.nmap.org' exceeded the maximum allowed time of 11
minutes and was terminated."*

### 2.3 Reading scans

| Endpoint | Returns |
|---|---|
| `GET /api/scans/{id}` | Scan with its full result set |
| `GET /api/scans` | Paginated list (page size 1–25), ordering and target filter |

Both are output-cached and scoped to the caller's team. The cache is evicted by
tag when a scan is created, so a new scan appears immediately.

Each result row is: port, protocol, service, state, product, version.

### 2.4 Scan diff

`GET /api/scans/diff/{target}` compares two **completed** scans of one target
and classifies every port:

| Bucket | Meaning |
|---|---|
| `addedPorts` | Present now, absent before — new exposure |
| `removedPorts` | Present before, absent now |
| `changedPorts` | Present in both, state changed (`oldState` → `newState`) |
| `unchangedPorts` | Identical in both |

Selection follows the parameters:

| Parameters | Compares |
|---|---|
| neither | the latest two completed scans |
| `from` only | that scan against the latest |
| `from` and `to` | exactly those two |

Before comparing, the API validates that both scans belong to the target, are
`Completed`, and are in the correct chronological order.

**In the UI** (`/diff`): choose a target, optionally pin From/To (default
"Previous (auto)" and "Latest (auto)"), and the four buckets render colour-coded
— green added, red removed, amber changed, muted unchanged.

---

## 3. Risk assessment

### 3.1 Requesting one

`POST /api/scans/{scanId}/risk-assessments` — path parameter, no body, plus an
idempotency key. Returns **202 Accepted**.

**Guardrail.** A DB-backed validation rule requires the scan to exist, be
`Completed`, and have at least one result. Otherwise **400**. The portal mirrors
this: the risk-assessment panel only renders for a completed scan with results,
so the button cannot be offered for an assessment that would be rejected.

### 3.2 What the engine does

For each scanned service:

1. **Resolve a CPE** from product + version, with a confidence score.
2. **Look up CVEs** for that CPE — local cache first (7-day TTL), NVD on a miss.
3. **Flag KEV** — mark CVEs present in CISA's Known Exploited Vulnerabilities
   catalogue.
4. **Score the finding** — CVSS (or a service heuristic when no CVE applies),
   multiplied by 1.15 if KEV, capped at 10.
5. **Aggregate** — overall = `0.7 × worst + 0.3 × average`.

### 3.3 Reading assessments

| Endpoint | Returns |
|---|---|
| `GET /api/risk-assessments` | Paginated list, optional status filter |
| `GET /api/risk-assessments/{id}` | One assessment with findings |
| `GET /api/scans/{scanId}/risk-assessments` | All assessments for a scan |
| `GET /api/scans/{scanId}/risk-assessment` | The latest one for a scan |

A finding contains: port, service, product/version, CPE, matched CVE ids, CVSS,
KEV flag, and match confidence. In the UI, CVSS is badged (green < 4, amber
4–6.9, red ≥ 7), confidence is shown as a percentage, and **each CVE id links to
its NVD detail page**.

### 3.4 Reading the numbers honestly

A finding with **no product/version** — nmap reports `tcpwrapped`, meaning the
port is open but the service returned no usable banner — produces no CPE, no
CVEs, CVSS 0.0 and confidence 0%. A resulting overall score of **0 is correct**:
it means *nothing identifiable was found*, not *this host is safe*. Absence of
evidence is reported as absence, not as safety.

Similarly, a **distro-patched** package (Ubuntu's backported Apache, say) cannot
be distinguished from an unpatched upstream release by banner alone. Vantage
does not pretend otherwise; it reports the match at a confidence level and
leaves the judgement visible.

---

## 4. Dashboard

### 4.1 Summary cards

`GET /api/dashboard/summary` returns team-scoped totals computed **in SQL**:
total scans, total assessments, completed assessments, and the average overall
risk score across completed assessments (one decimal, arithmetic rounding).

These are exact at any volume. An earlier implementation derived them in the
browser from a 25-item page, which silently under-reported once a team had more
than 25 scans.

### 4.2 Activity chart

`GET /api/dashboard/activity?from=&to=&bucket=day|hour` returns time-bucketed
counts of **scans and assessments** over a range, gap-filled so empty buckets
appear as zero rather than vanishing.

In the UI: a **24h / 7d / 14d / 30d** range selector (24h switches to hourly
buckets) and a **clickable legend** to toggle each series. Scans render white,
assessments amber — deliberately distinct hues, because the two series were
previously indistinguishable in the monochrome palette.

Server-side bounds cap a day-bucketed range at 90 days and an hour-bucketed one
at 3 days, so a hand-crafted query cannot ask for an unbounded scan of the
table.

---

## 5. System status

`GET /api/health` (anonymous) reports the database plus each background service.
A service counts as **up** only if its heartbeat row was written within the last
30 seconds; each worker upserts its heartbeat every 10 seconds.

Because the heartbeat is a *database write*, a green indicator proves the worker
is both alive **and** able to reach the database — a liveness signal that cannot
be faked by a process that is running but wedged.

The `/status` page lists Scanning and Assessment with a coloured dot, an
ONLINE/OFFLINE label, hover tooltips, and a "last checked" age, refreshing every
10 seconds. Killing the Go container turns Assessment red within 30 seconds and
it recovers on restart.

---

## 6. Authentication, tenancy and identity

### 6.1 Signing in

Clerk provides sign-in, sign-up, organisations, roles, invitations and the
account UI. Signed-out users cannot reach the app. A signed-in user with **no
active organisation** gets a gate offering create-or-select, because every
resource in Vantage belongs to a team.

### 6.2 Auth modes

`Auth__Mode` selects authentication independently of the environment:

| Mode | Behaviour |
|---|---|
| `DevHeaders` | Trusts `X-Dev-Subject` / `X-Dev-Org` / `X-Dev-Role` / `X-Dev-Email`. **Refuses to start outside Development.** |
| `Clerk` | Validates real Clerk JWTs against `Clerk__Authority`. Fails fast at start-up if the authority is missing. |

Blank defaults to `DevHeaders` in Development and `Clerk` elsewhere. Because the
two are decoupled, real token validation can be exercised locally **with Swagger
still available** — previously impossible, which is why it had gone untested.

Under `Auth__Mode=Clerk`, dev headers and malformed tokens are rejected with
**401**; a genuine browser-issued Clerk JWT is accepted.

### 6.3 Tenant isolation

Every scan and assessment query is scoped to the caller's team by an EF Core
**global query filter**. There are no per-handler tenant checks — isolation is a
property of the data layer, so a newly added endpoint is isolated by default
rather than by remembering.

An authenticated request with no active organisation is refused with **403
before reaching a controller**, because the filter's null-bypass exists for
background workers and must never be reachable from the web.

### 6.4 Roster synchronisation

**Just-in-time**, on every request: the caller's user, team and membership are
upserted from token claims. Only non-null values overwrite stored data, so a
token missing a claim never blanks existing information.

**Webhooks**, for everyone else: `POST /api/webhooks/clerk` (anonymous, Svix
verified) handles

- `user.created` / `user.updated` / `user.deleted`
- `organization.created` / `organization.updated` / `organization.deleted`
- `organizationMembership.created` / `.updated` / `.deleted`

This is what makes members who have **never opened the app** appear with the
right role, and what makes **removals propagate** — neither of which JIT sync
can do.

Two deliberate behaviours:

- **`user.deleted` revokes memberships but keeps the user row.** Access is
  removed immediately; authorship history survives. (`Scan.CreatedByUserId` is
  `Restrict`, so deleting the user would fail anyway once they had scanned.)
- **`organization.deleted` deletes the team**, cascading that tenant's scans and
  assessments. The tenant no longer exists and the rows would otherwise be
  permanently unreachable behind the team filter.

Unknown event types are acknowledged with **200** and ignored, so Svix does not
retry events the system does not model.

### 6.5 Webhook security

Every delivery must carry a valid Svix signature:
`HMAC-SHA256({svix-id}.{svix-timestamp}.{raw body})`, compared in **constant
time**, within a **±5-minute** window.

| Condition | Response |
|---|---|
| Valid signature | 200, handler runs |
| Invalid / absent signature | 401, handler never runs |
| Tampered body | 401 |
| Stale timestamp (replay) | 401 |
| No signing secret configured | **503** — refuses to accept unverified events |

The 503 matters: an unconfigured endpoint fails closed rather than silently
trusting whatever arrives.

---

## 7. API reference

| Method | Path | Auth | Purpose |
|---|---|---|---|
| `GET` | `/api/health` | anonymous | DB + worker liveness |
| `POST` | `/api/scans` | required | Queue a scan → 202 |
| `GET` | `/api/scans` | required | Paginated list |
| `GET` | `/api/scans/{id}` | required | Scan + results |
| `GET` | `/api/scans/diff/{target}` | required | Compare two scans |
| `POST` | `/api/scans/{scanId}/risk-assessments` | required | Queue assessment → 202 |
| `GET` | `/api/scans/{scanId}/risk-assessments` | required | All for a scan |
| `GET` | `/api/scans/{scanId}/risk-assessment` | required | Latest for a scan |
| `GET` | `/api/risk-assessments` | required | Paginated list |
| `GET` | `/api/risk-assessments/{id}` | required | One + findings |
| `GET` | `/api/dashboard/summary` | required | Exact totals |
| `GET` | `/api/dashboard/activity` | required | Time-bucketed counts |
| `POST` | `/api/webhooks/clerk` | Svix signature | Roster sync |

**Status codes:** 202 accepted work, 200 reads, 400 validation (with the
specific message), 401 unauthenticated, 403 no active organisation, 404 unknown
id, 503 webhook endpoint unconfigured.

---

## 8. The portal

| Route | Purpose |
|---|---|
| `/` | Dashboard — summary cards + activity chart |
| `/scans` | Scans list, target filter, "New scan", live polling |
| `/scans/:id` | Scan detail, results table, risk-assessment panel |
| `/diff` | Scan comparison |
| `/status` | Per-service liveness |

The avatar sits top-right and opens Clerk's account modal; the organisation
switcher sits in the sidebar footer. Status badges are colour-coded: Pending
muted, Running blue/animated, Completed green, Failed red. Lists poll while work
is in flight and stop once everything is terminal.

---

## 9. Quality: the CPE evaluation harness

The CPE resolver is the project's substantive technical claim, so it is
**measured**, not asserted.

`go run ./cmd/cpe-eval [-failures]` scores the resolver against a naive baseline
over a hand-labelled corpus of 34 real `nmap -sV` banners drawn from
Metasploitable2, DVWA, common services, and a **held-out** set of products
deliberately absent from the dictionary.

A sample counts as a true positive only if the emitted CPE matches the label
**exactly** *and* clears the confidence threshold; banners with no catalogued
product must be abstained on.

| resolver | threshold | precision | recall | F1 |
|---|---|---|---|---|
| naive | 0.00 | 0.333 | 1.000 | 0.500 |
| dictionary | 0.00 | 0.818 | 1.000 | 0.900 |
| dictionary | 0.50 | **1.000** | 0.742 | 0.852 |
| dictionary | 0.90 | 1.000 | 0.677 | 0.808 |

**What it demonstrates.** The dictionary plus version normalisation nearly
doubles F1 at identical recall — the naive baseline collapses on distro-patched
versions and compound product strings. The confidence tier is what buys
precision: rejecting below 0.5 eliminates every false positive, which means the
score is a usable abstention signal rather than decoration.

**What it found.** The harness surfaced two real defects, since fixed: MySQL was
mapped to `mysql:mysql` when NVD publishes `oracle:mysql` (so MySQL CVE lookups
silently returned nothing), and Tomcat's actual banner
`Apache Tomcat/Coyote JSP engine` fell back to `apache:apache`.

**Its honest limits.** The corpus is small and hand-labelled, and the dictionary
was corrected in response to it, so in-dictionary figures are optimistic; the
held-out block is the real generalisation signal. It also revealed an open
calibration gap: four of the eight rejections at 0.5 (OpenLDAP, HAProxy,
Memcached, Dovecot) emitted the **correct** CPE at 0.40 confidence. The fallback
is right more often than it admits whenever vendor equals product — raising that
case would recover recall at no cost to precision. This was left unchanged
deliberately, to avoid tuning the algorithm to its own test set.

---

## 10. Designed but not yet implemented

### 10.1 Clerk webhook delivery wiring
The endpoint, verification and handlers are complete and tested. What remains is
operational: create the endpoint in the Clerk dashboard, copy its signing secret
into `CLERK_WEBHOOK_SIGNING_SECRET`, and expose the API publicly
(`clerk webhooks listen` or a tunnel) so events actually arrive.

### 10.2 Clerk session-token custom claims
Clerk's v2 session token carries no `email` or `name`, so under
`Auth__Mode=Clerk` the "created by" column is blank. Adding
`{"email": "{{user.primary_email_address}}", "name": "{{user.full_name}}"}` to
the session token in the Clerk dashboard fills it. Cosmetic only — auth works
without it.

### 10.3 CPE confidence calibration
Raise fallback confidence when vendor equals product, and widen the corpus so
the change is validated against data it was not derived from.

### 10.4 Full NVD mirror
Today: on-demand fetch, 7-day cache, self-throttled client. A complete offline
sync would remove the cold-start network call entirely and make assessments
fully deterministic.

### 10.5 Risk-trend UI
Assessment rows already carry `overallRiskScore` and timestamps; a per-target
score-over-time chart would reuse the activity chart's range selector.

### 10.6 Broader scan configurability
The nmap argument list is fixed. Port ranges, timing templates and scan types
are not user-selectable — a deliberate safety boundary, since accepting
arbitrary nmap flags from a web request is an obvious injection surface. Any
future exposure should be an allow-list of named profiles, never raw flags.

### 10.7 Known nit
On a cold `docker compose up`, vulnintel logs one heartbeat error before
migrations have been applied, then self-heals.

### 10.8 Environmental limits (not defects)
In a sandboxed or egress-filtered network, external scan traffic is dropped:
every port reads `filtered`, so external scans complete with empty results or
`tcpwrapped` services. Internal targets on the compose network are unaffected.
This is a property of the network, not of Vantage — the graceful host-timeout
means such scans now finish as `Completed` rather than dying at the backstop.

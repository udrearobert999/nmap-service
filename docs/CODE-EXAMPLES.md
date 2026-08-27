# Vantage — Code Examples

Short, real listings drawn from the Vantage codebase, organised from
foundational patterns through to concrete business logic. Every snippet is
lightly trimmed (imports, logging and error noise removed) for readability but
is faithful to the actual implementation, so it can be pasted into the thesis as
`Listing X.Y`.

Sections:
1. Persistence patterns — Repository, Unit of Work, Result
2. Multi-tenancy — the global query filter
3. Reliability — transactional outbox, atomic claim, idempotency
4. Dispatch — Strategy over the outbox
5. Resilience — health heartbeat, NVD rate limiting and retry
6. Ports and adapters — the Go domain interfaces
7. Business logic — the risk-assessment pipeline
8. Business logic — CPE resolution (anti-corruption layer)
9. Business logic — cache-aside CVE lookup
10. Business logic — risk scoring
11. Security — Svix webhook verification, org-claim resolution
12. Composition — convention-based dependency injection

---

## 1. Persistence patterns

### 1.1 The repository abstraction (Domain layer, no framework)

```csharp
public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default,
        bool track = false);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default);
    // GetAllAsync, GetByExpressionAsync, GetByIdAsync, ExistsAsync omitted
}

public interface IRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
```

The interfaces live in the Domain layer and mention no EF Core type. Splitting
read from write lets a query path be typed so that it cannot mutate.

### 1.2 The Result pattern — expected failure as a value, not an exception

```csharp
public record Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => _value ?? throw new InvalidOperationException("Result has no value.");

    public static Result<TValue> Success(TValue value) => new(value);
    public new static Result<TValue> NotFound() => new(ErrorFactory.NotFound());
    public new static Result<TValue> ValidationFailure(string message) =>
        new(ErrorFactory.ValidationFailure(message));

    public static implicit operator Result<TValue>(TValue value) => new(value);
}
```

A validation failure or a missing row is an ordinary outcome, so it is modelled
as a value. The controller base then maps a `Result` to an HTTP status in one
place rather than each handler catching exceptions.

---

## 2. Multi-tenancy — the global query filter

```csharp
// AppDbContext.OnModelCreating
modelBuilder.Entity<Scan>()
    .HasQueryFilter(s =>
        _currentTeamAccessor.TeamId == null || s.TeamId == _currentTeamAccessor.TeamId);

modelBuilder.Entity<ScanRiskAssessment>()
    .HasQueryFilter(r =>
        _currentTeamAccessor.TeamId == null || r.TeamId == _currentTeamAccessor.TeamId);
```

Every query on these entities is scoped to the current team automatically. There
are no per-handler `WHERE TeamId = …` checks anywhere in the codebase, so a new
endpoint is isolated by default. The `== null` branch is the deliberate bypass
for background workers, which have no active team — and the reason a web request
with no active organisation must be rejected before it reaches a controller.

---

## 3. Reliability

### 3.1 Transactional outbox — business row and intent in one transaction

```csharp
public async Task<Result<CreateScanResponseDto>> CreateAsync(
    IdempotentCreateScanRequestDto request, CancellationToken ct = default)
{
    if (_currentTeamAccessor.TeamId is not { } teamId ||
        _currentUserAccessor.UserId is not { } userId)
        return Result<CreateScanResponseDto>.ValidationFailure("Unable to resolve the current team.");

    var validation = await _validationOrchestrator.ValidateAsync(request, ct);
    if (validation.IsFailure)
        return Result<CreateScanResponseDto>.ValidationFailure(validation.Error);

    // Idempotency fast path: same key -> return the original scan.
    var existing = await _unitOfWork.Scans
        .FirstOrDefaultAsync(s => s.RequestId == request.RequestId, ct);
    if (existing is not null)
        return Result<CreateScanResponseDto>.Success(existing.ToCreateResponse());

    var scan = request.ToEntity(teamId, userId);
    var outboxMessage = scan.ToOutboxMessage();

    await _unitOfWork.Scans.CreateAsync(scan, ct);
    await _unitOfWork.OutboxMessages.CreateAsync(outboxMessage, ct);

    return await ConcurrencySafeSaveAsync(scan, request.RequestId, ct); // single SaveChanges
}
```

The scan row and the outbox row are written by one `SaveChangesAsync`, so they
commit together or not at all. There is no window in which a scan exists without
its intent to publish, which is what defeats the dual-write problem.

### 3.2 Idempotency under a genuine race

```csharp
private async Task<Result<CreateScanResponseDto>> ConcurrencySafeSaveAsync(
    Scan scan, Guid requestId, CancellationToken ct)
{
    try
    {
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<CreateScanResponseDto>.Success(scan.ToCreateResponse());
    }
    catch (Exception ex) when (_unitOfWork.IsUniqueConstraintViolation(ex))
    {
        // Two identical requests raced past the fast-path check; the unique
        // RequestId index rejected the loser. Return the winner's row.
        var existing = await _unitOfWork.Scans
            .FirstOrDefaultAsync(s => s.RequestId == requestId, ct);
        return Result<CreateScanResponseDto>.Success(existing!.ToCreateResponse());
    }
}
```

The check in 3.1 handles the common case; this catch handles the true race in
which both requests pass the check before either commits. Idempotency is
therefore guaranteed by the database, not merely by the read.

### 3.3 Atomic claim of the outbox — `FOR UPDATE SKIP LOCKED`

```csharp
public async Task<IList<OutboxMessage>> ClaimScanAsync(
    int batchSize, CancellationToken ct = default)
{
    var sql = $"""
        UPDATE "{TableNamesConstants.OutboxMessages}"
        SET "Status" = '{Status.Running}'
        WHERE "Id" IN (
            SELECT "Id" FROM "{TableNamesConstants.OutboxMessages}"
            WHERE "Status" = '{Status.Pending}'
            ORDER BY "CreatedAt"
            LIMIT {batchSize}
            FOR UPDATE SKIP LOCKED
        )
        RETURNING *;
        """;

    return await _dbSet.FromSqlRaw(sql).ToListAsync(ct);
}
```

`SKIP LOCKED` lets a second publisher instance skip rows the first has locked
rather than blocking on them, so the outbox scales across instances without any
coordination code.

### 3.4 Atomic claim of a scan — the winner-takes-it update

```csharp
public async Task<bool> ClaimScanAsync(Guid id, CancellationToken ct = default)
{
    var rowsAffected = await _dbSet
        .Where(s => s.Id == id && s.Status == Status.Pending)
        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, Status.Running), ct);

    return rowsAffected > 0; // only the caller that flipped the row proceeds
}
```

When Kafka redelivers a message, the second worker finds the row already
`Running`, gets `rowsAffected == 0`, and stops. Making the read part of the write
closes the check-then-act window.

---

## 4. Dispatch — Strategy over the outbox

### 4.1 The publisher port

```csharp
// One implementation per OutboxMessage.Type, so the job can dispatch a claimed
// message to the right Kafka producer without hard-coding a single type.
internal interface IOutboxMessagePublisher
{
    string MessageType { get; }
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}
```

### 4.2 The dispatch itself

```csharp
public async Task Execute(IJobExecutionContext context)   // Quartz, every 2s
{
    var messages = await _unitOfWork.OutboxMessages.ClaimScanAsync(20, context.CancellationToken);
    if (messages.Count == 0) return;

    await Parallel.ForEachAsync(messages, GetParallelOptions(context.CancellationToken),
        async (message, token) =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var publishers = scope.ServiceProvider.GetServices<IOutboxMessagePublisher>();
            await TryProcessSingleMessageAsync(message, uow, publishers, token);
        });
}

private async Task TryProcessSingleMessageAsync(
    OutboxMessage msg, IUnitOfWork uow,
    IEnumerable<IOutboxMessagePublisher> publishers, CancellationToken ct)
{
    var publisher = publishers.FirstOrDefault(p => p.MessageType == msg.Type);
    if (publisher is null)
    {
        await uow.OutboxMessages.MarkAsFailedAsync(
            msg.Id, $"No publisher registered for type '{msg.Type}'.", ct);
        return;
    }

    await publisher.PublishAsync(msg, ct);
    await uow.OutboxMessages.MarkAsCompletedAsync(msg.Id, ct);
}
```

`OutboxMessage.Type` selects the publisher, so one table and one job serve both
the MassTransit scan topic and the raw-JSON assessment topic that the Go service
consumes. Adding a message type is a new class, not a change to this job.

---

## 5. Resilience

### 5.1 Heartbeat upsert — liveness as a database write

```csharp
public async Task UpsertAsync(string serviceName, CancellationToken ct = default)
{
    var sql =
        "INSERT INTO \"" + TableNamesConstants.ServiceHeartbeats + "\" (\"ServiceName\", \"LastSeenAt\") " +
        "VALUES ({0}, now()) " +
        "ON CONFLICT (\"ServiceName\") DO UPDATE SET \"LastSeenAt\" = now();";

    await _dbContext.Database.ExecuteSqlRawAsync(sql, [serviceName], ct);
}
```

Because liveness is recorded by writing to the database, `GET /api/health` being
green proves a worker is both running and able to reach PostgreSQL — something an
in-process HTTP ping could not establish.

### 5.2 NVD client — rate limiting with a monotonic next-slot clock

```go
func (c *Client) awaitSlot(ctx context.Context) error {
    c.mu.Lock()
    now := time.Now()
    wait := time.Duration(0)
    if c.nextAllowed.After(now) {
        wait = c.nextAllowed.Sub(now)
    }
    c.nextAllowed = now.Add(wait + c.minInterval) // reserve this caller's slot
    c.mu.Unlock()

    return sleepCtx(ctx, wait) // context-aware: a cancelled assessment stops waiting
}
```

Concurrent assessments queue behind a shared next-slot time rather than bursting
past NVD's quota. The interval is ~6.5 s anonymously, ~0.7 s with an API key.

### 5.3 NVD client — retry classification and backoff

```go
func isRetryable(err error) bool {
    switch typed := err.(type) {
    case transportError:
        return true
    case statusError:
        return typed.status == http.StatusTooManyRequests || // 429
            typed.status == http.StatusForbidden ||           // 403 (quota)
            typed.status >= http.StatusInternalServerError    // 5xx
    default:
        return false // a 4xx is a bad request; retrying only burns quota
    }
}

func backoffFor(attempt int, err error) time.Duration {
    if typed, ok := err.(statusError); ok && typed.retryAfter > 0 {
        return capBackoff(typed.retryAfter) // honour Retry-After
    }
    delay := baseBackoff << (attempt - 1)                 // exponential
    jitter := time.Duration(rand.Int63n(int64(baseBackoff))) // decorrelate retries
    return capBackoff(delay + jitter)
}
```

Only throttling and server faults retry; a malformed request fails fast. Jitter
stops many clients retrying in lockstep and re-synchronising into a second spike.

---

## 6. Ports and adapters — the Go domain interfaces

```go
// internal/domain — ports only, implemented by the adapter packages.
type CPEResolver interface {
    Resolve(product, version string) ResolvedCPE
}
type CVELookup interface {
    LookupByCPE(ctx context.Context, cpeURI string) ([]CVE, error)
}
type KEVCatalog interface {
    IsKEV(cveID string) bool
}
type Scorer interface {
    ScoreFinding(finding Finding) float64
    Overall(findingScores []float64) float64
}
```

```go
// Compile-time proof that an adapter still satisfies its port.
var _ domain.Scorer = (*Scorer)(nil)
var _ domain.CVELookup = (*CachedLookup)(nil)
```

The assertion turns a drifted signature into a build error rather than a runtime
surprise.

---

## 7. Business logic — the risk-assessment pipeline

```go
func (s *scanRiskAssessmentService) Assess(
    ctx context.Context, event domain.ScanRiskAssessmentRequested) error {

    findings := make([]domain.Finding, 0, len(event.Results))
    scores := make([]float64, 0, len(event.Results))

    for _, result := range event.Results {
        finding := s.assessResult(ctx, result)        // CPE -> CVE -> KEV
        findings = append(findings, finding)
        scores = append(scores, s.scorer.ScoreFinding(finding))
    }

    overall := s.scorer.Overall(scores)

    if err := s.findingsWriter.SaveFindings(ctx, event.ScanRiskAssessmentID, findings); err != nil {
        _ = s.statusWriter.MarkFailed(ctx, event.ScanRiskAssessmentID, err.Error())
        return err
    }
    return s.statusWriter.MarkCompleted(ctx, event.ScanRiskAssessmentID, overall)
}
```

The orchestration reads top to bottom and depends only on ports, so it can be
followed without any knowledge of PostgreSQL, NVD or Kafka.

```go
func (s *scanRiskAssessmentService) assessResult(
    ctx context.Context, result domain.ScanResult) domain.Finding {

    resolved := s.cpeResolver.Resolve(result.Product, result.Version)

    var cves []domain.CVE
    if resolved.URI != "" {
        if matches, err := s.cveLookup.LookupByCPE(ctx, resolved.URI); err == nil {
            cves = matches
        } // a lookup error degrades to "no CVEs", it does not fail the assessment
    }

    matchedIDs := make([]string, 0, len(cves))
    maxCvss, kevFlag := 0.0, false
    for _, cve := range cves {
        matchedIDs = append(matchedIDs, cve.ID)
        if cve.CVSS > maxCvss {
            maxCvss = cve.CVSS
        }
        if s.kevCatalog.IsKEV(cve.ID) {
            kevFlag = true
        }
    }

    return domain.Finding{
        Port: result.Port, Service: result.Service,
        Product: result.Product, Version: result.Version,
        CPE: resolved.URI, MatchedCVEs: matchedIDs,
        CVSSScore: maxCvss, KEVFlag: kevFlag,
        MatchConfidence: resolved.Confidence,
    }
}
```

---

## 8. Business logic — CPE resolution (anti-corruption layer)

```go
func (r *Resolver) Resolve(product, version string) domain.ResolvedCPE {
    normalizedProduct := strings.ToLower(strings.TrimSpace(product))
    normalizedVersion := normalizeVersion(version)

    if normalizedProduct == "" {
        return domain.ResolvedCPE{URI: "", Confidence: 0}
    }

    if vp, ok := dictionary[normalizedProduct]; ok { // curated vendor:product
        return build(vp.vendor, vp.product, normalizedVersion,
            dictionaryConfidence(normalizedVersion)) // 0.9 with version, else 0.6
    }

    token := fallbackProductToken(normalizedProduct) // first word as vendor+product
    return build(token, token, normalizedVersion,
        fallbackConfidence(normalizedVersion))       // 0.4 with version, else 0.2
}

// "6.6.1p1 Ubuntu 2ubuntu2.13" -> "6.6.1p1"; "8.3.0 - 8.3.7" -> "8.3.0"
var versionPattern = regexp.MustCompile(`[0-9]+(\.[0-9]+)*([a-z][0-9a-z]*)?`)

func normalizeVersion(version string) string {
    return versionPattern.FindString(strings.TrimSpace(version))
}
```

The dictionary exists because the vendor is frequently not the product name
(`mysql` → `oracle:mysql`); the confidence value carries the uncertainty forward
honestly instead of discarding it.

---

## 9. Business logic — cache-aside CVE lookup

```go
func (l *CachedLookup) LookupByCPE(ctx context.Context, cpeURI string) ([]domain.CVE, error) {
    if cpeURI == "" {
        return nil, nil
    }

    cached, err := l.store.GetCachedCVEs(ctx, cpeURI, time.Now().Add(-l.ttl)) // 7-day TTL
    if err == nil && len(cached) > 0 {
        return cached, nil // hit: no network call
    }

    fresh, err := l.nvd.LookupByCPE(ctx, cpeURI)
    if err != nil {
        // Graceful degradation: NVD down -> return whatever the cache had.
        return cached, nil
    }

    if len(fresh) > 0 {
        _ = l.store.PutCachedCVEs(ctx, cpeURI, fresh) // populate for next time
    }
    return fresh, nil
}
```

A repeat assessment of the same product makes no network call, and an NVD outage
degrades the answer rather than failing the assessment.

---

## 10. Business logic — risk scoring

```go
const kevMultiplier = 1.15

func (s *Scorer) ScoreFinding(finding domain.Finding) float64 {
    base := finding.CVSSScore
    if len(finding.MatchedCVEs) == 0 {
        base = heuristicBase(finding.Service) // dangerous-but-unversioned service
    }
    score := base
    if finding.KEVFlag {
        score *= kevMultiplier // known-exploited outranks severity alone
    }
    return round1(math.Min(score, 10))
}

func (s *Scorer) Overall(findingScores []float64) float64 {
    if len(findingScores) == 0 {
        return 0
    }
    worst, sum := 0.0, 0.0
    for _, v := range findingScores {
        worst = math.Max(worst, v)
        sum += v
    }
    average := sum / float64(len(findingScores))
    return round1(math.Min(0.7*worst+0.3*average, 10)) // one critical dominates,
                                                        // breadth still counts
}

func heuristicBase(service string) float64 {
    switch strings.ToLower(service) {
    case "telnet":
        return 7.0
    case "rdp", "ms-wbt-server", "vnc":
        return 6.0
    case "ftp", "ftp-data", "smb", "microsoft-ds", "netbios-ssn":
        return 5.0
    default:
        return 0.0
    }
}
```

---

## 11. Security

### 11.1 Svix webhook signature verification (constant time, replay window)

```csharp
public static bool Verify(
    string? signingSecret, string? id, string? timestamp,
    string? signatureHeader, string payload, DateTimeOffset now)
{
    if (string.IsNullOrWhiteSpace(signingSecret) || string.IsNullOrWhiteSpace(id) ||
        string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signatureHeader))
        return false;

    if (!IsTimestampFresh(timestamp, now))     // +/- 5 minutes: defeats replay
        return false;
    if (!TryDecodeSecret(signingSecret, out var secret))
        return false;

    var signedContent = $"{id}.{timestamp}.{payload}";
    using var hmac = new HMACSHA256(secret);
    var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));

    foreach (var candidate in signatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    {
        var sep = candidate.IndexOf(',');
        if (sep < 0 || candidate[..sep] != "v1") continue;
        if (!TryDecodeBase64(candidate[(sep + 1)..], out var provided)) continue;

        if (CryptographicOperations.FixedTimeEquals(expected, provided)) // no timing leak
            return true;
    }
    return false;
}
```

A naive `==` would return on the first differing byte and leak, over many
attempts, enough timing signal to forge a signature; `FixedTimeEquals` does not.
An unconfigured secret makes the endpoint fail closed with 503 elsewhere.

### 11.2 Resolving the org from Clerk's v2 nested claim

```csharp
private static (string? OrgId, string? OrgRole) ResolveOrganization(ClaimsPrincipal user)
{
    var flatOrgId = user.FindFirst("org_id")?.Value;      // dev headers / legacy
    if (!string.IsNullOrWhiteSpace(flatOrgId))
        return (flatOrgId, NormalizeRole(user.FindFirst("org_role")?.Value));

    var organizationClaim = user.FindFirst("o")?.Value;    // Clerk v2: {"id","rol"}
    if (string.IsNullOrWhiteSpace(organizationClaim))
        return (null, null);

    using var document = JsonDocument.Parse(organizationClaim);
    var root = document.RootElement;
    var id = root.TryGetProperty("id", out var e) ? e.GetString() : null;
    var role = root.TryGetProperty("rol", out var r) ? r.GetString() : null;
    return (id, NormalizeRole(role)); // strips an "org:" prefix
}
```

Reading both claim shapes is what let real JWT validation work: the v2 token
nests the active organisation under `o` rather than exposing a flat `org_id`.

---

## 12. Composition — convention-based dependency injection

```csharp
// Every class named *Service is registered by convention; every IValidator<>
// is discovered by assembly scan. Adding a service needs no registration line.
services.Scan(scan => scan
    .FromAssemblies(currentAssembly)
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")), false)
    .AsMatchingInterface()
    .WithScopedLifetime());

services.AddValidatorsFromAssembly(currentAssembly, ServiceLifetime.Transient);
```

The most common way to break dependency injection — forgetting to register a new
service — is designed out.
```

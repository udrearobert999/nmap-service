using System.Data;
using Vantage.Domain.Entities;

namespace Vantage.Domain.Abstractions;

public interface IUnitOfWork
{
    public IScanRepository Scans { get; set; }
    public IOutboxMessageRepository OutboxMessages { get; set; }
    public IRepository<ScanResult, Guid> ScanResults { get; set; }
    public IScanRiskAssessmentRepository ScanRiskAssessments { get; set; }
    public IRepository<Team, Guid> Teams { get; set; }
    public IRepository<User, Guid> Users { get; set; }
    public IRepository<TeamMembership, Guid> TeamMemberships { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public bool IsUniqueConstraintViolation(Exception exception);
    public void Detach(object entity);
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
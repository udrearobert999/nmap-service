using Microsoft.EntityFrameworkCore;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;

namespace Vantage.Infrastructure.Persistence;

internal sealed class AppDbContext : DbContext
{
    private readonly ICurrentTeamAccessor _currentTeamAccessor;

    public AppDbContext(DbContextOptions options, ICurrentTeamAccessor currentTeamAccessor) : base(options)
    {
        _currentTeamAccessor = currentTeamAccessor;
    }

    public required DbSet<Scan> Scans { get; set; }
    public required DbSet<ScanResult> ScanResults { get; set; }
    public required DbSet<OutboxMessage> OutboxMessages { get; set; }
    public required DbSet<ScanRiskAssessment> ScanRiskAssessments { get; set; }
    public required DbSet<Team> Teams { get; set; }
    public required DbSet<ServiceHeartbeat> ServiceHeartbeats { get; set; }
    public required DbSet<ScanRiskAssessmentFinding> ScanRiskAssessmentFindings { get; set; }
    public required DbSet<CveCacheEntry> CveCacheEntries { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<TeamMembership> TeamMemberships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        modelBuilder.Entity<Scan>()
            .HasQueryFilter(s => _currentTeamAccessor.TeamId == null || s.TeamId == _currentTeamAccessor.TeamId);

        modelBuilder.Entity<ScanRiskAssessment>()
            .HasQueryFilter(r => _currentTeamAccessor.TeamId == null || r.TeamId == _currentTeamAccessor.TeamId);
    }
}
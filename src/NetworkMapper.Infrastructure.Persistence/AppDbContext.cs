using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Persistence;

internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public required DbSet<Scan> Scans { get; set; }
    public required DbSet<ScanResult> ScansResults { get; set; }
    public required DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
}
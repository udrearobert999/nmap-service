using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class ScanConfiguration : IEntityTypeConfiguration<Scan>
{
    public void Configure(EntityTypeBuilder<Scan> builder)
    {
        builder.ToTable(TableNamesConstants.Scans);

        builder.HasKey(x => x.Id);
        
        builder.HasIndex(s => s.RequestId)
            .IsUnique();

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Property(x => x.Target)
            .IsRequired();
        
        builder
            .Property(x => x.Status)
            .IsRequired();
        
        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class ScanResultConfiguration : IEntityTypeConfiguration<ScanResult>
{
    public void Configure(EntityTypeBuilder<ScanResult> builder)
    {
        builder.ToTable(TableNamesConstants.ScansResults);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.Protocol).IsRequired();
        builder.Property(x => x.Service).IsRequired();
        builder.Property(x => x.State).IsRequired();

        builder.HasOne(x => x.Scan)
            .WithMany(s => s.Results)
            .HasForeignKey(x => x.ScanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
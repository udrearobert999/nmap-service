using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vantage.Domain.Entities;
using Vantage.Infrastructure.Persistence.Constants;

namespace Vantage.Infrastructure.Persistence.Configurations;

internal sealed class ScanResultConfiguration : IEntityTypeConfiguration<ScanResult>
{
    public void Configure(EntityTypeBuilder<ScanResult> builder)
    {
        builder.ToTable(TableNamesConstants.ScanResults);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.Protocol).IsRequired();
        builder.Property(x => x.Service).IsRequired();
        builder.Property(x => x.State).IsRequired();
        builder.Property(x => x.Product).IsRequired(false);
        builder.Property(x => x.Version).IsRequired(false);

        builder.HasOne(x => x.Scan)
            .WithMany(s => s.Results)
            .HasForeignKey(x => x.ScanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
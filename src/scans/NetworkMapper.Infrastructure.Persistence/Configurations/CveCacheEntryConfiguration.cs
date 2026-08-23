using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class CveCacheEntryConfiguration : IEntityTypeConfiguration<CveCacheEntry>
{
    public void Configure(EntityTypeBuilder<CveCacheEntry> builder)
    {
        builder.ToTable(TableNamesConstants.CveCacheEntries);

        builder.HasKey(x => new { x.CpeUri, x.CveId });

        builder.Property(x => x.CpeUri).IsRequired();
        builder.Property(x => x.CveId).IsRequired();
        builder.Property(x => x.CvssScore).IsRequired();
        builder.Property(x => x.CachedAt).IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class ServiceHeartbeatConfiguration : IEntityTypeConfiguration<ServiceHeartbeat>
{
    public void Configure(EntityTypeBuilder<ServiceHeartbeat> builder)
    {
        builder.ToTable(TableNamesConstants.ServiceHeartbeats);

        builder.HasKey(x => x.ServiceName);

        builder.Property(x => x.ServiceName).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
    }
}
